using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using PhotoEditor.Models;
using PhotoEditor.Services;
using PhotoEditor.Utils;

namespace PhotoEditor
{
    public class CliCommands
    {
        private readonly Filters filters = new Filters();
        private readonly ImageEditor editor = new ImageEditor();
        private readonly CollageMaker collageMaker = new CollageMaker();
        private readonly List<Photo> photos;

        public CliCommands(List<Photo> photos)
        {
            this.photos = photos;
        }

        public void PrintHelp()
        {
            Console.WriteLine("=== Справка по командам Photo Editor CLI ===\n");
            Console.WriteLine("add <путь>                  — добавить фото");
            Console.WriteLine("list                        — список фото");
            Console.WriteLine("open <id>                   — открыть фото в Windows");
            Console.WriteLine("save                        — сохранить состояние");
            Console.WriteLine("clear                       — очистить состояние");
            Console.WriteLine("brightness <id> <f>         — изменить яркость (0.5..2.0)");
            Console.WriteLine("color <id> <r> <g> <b>      — изменить цвет (0..255)");
            Console.WriteLine("crop <id> <x> <y> <w> <h>  — обрезать изображение");
            Console.WriteLine("rotate <id> <угол>          — повернуть изображение (градусы)");
            Console.WriteLine("pixel <id> <x> <y> <r> <g> <b> — изменить цвет пикселя");
            Console.WriteLine("collage <id1,id2,...> <путь> — создать коллаж и сохранить");
            Console.WriteLine("help                        — вывести справку по командам");
            Console.WriteLine("exit                        — выход с сохранением");
        }

        public void AddPhoto(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Использование: add <путь_к_файлу>");
                return;
            }

            Photo photo = new Photo(parts[1]);
            photo.LoadImage();
            photos.Add(photo);

            Console.WriteLine($"Добавлено: {photo.Name}");
        }

        public void ListPhotos()
        {
            if (photos.Count == 0)
            {
                Console.WriteLine("Список фото пуст");
                return;
            }

            for (int i = 0; i < photos.Count; i++)
                Console.WriteLine($"{i + 1}. {photos[i].Name}");
        }

        public void OpenPhoto(string[] parts)
        {
            if (parts.Length < 2 || !int.TryParse(parts[1], out int index))
            {
                Console.WriteLine("Использование: open <id>");
                return;
            }

            if (index < 1 || index > photos.Count)
            {
                Console.WriteLine("Неверный ID");
                return;
            }

            Photo photo = photos[index - 1];

            if (photo.Image == null)
                photo.LoadImage();

            Process.Start(new ProcessStartInfo
            {
                FileName = photo.FilePath,
                UseShellExecute = true
            });
        }

        public void ApplyBrightness(string[] parts)
        {
            if (parts.Length < 3 || !int.TryParse(parts[1], out int id) || !float.TryParse(parts[2], out float factor))
            {
                Console.WriteLine("Использование: brightness <id> <factor>");
                return;
            }

            Photo photo = GetPhotoById(id);
            photo.SetImage(filters.ApplyBrightness(photo.Image, factor));
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Яркость фото {photo.Name} изменена на {factor}");
        }

        public void ApplyColor(string[] parts)
        {
            if (parts.Length < 5 ||
                !int.TryParse(parts[1], out int id) ||
                !int.TryParse(parts[2], out int r) ||
                !int.TryParse(parts[3], out int g) ||
                !int.TryParse(parts[4], out int b))
            {
                Console.WriteLine("Использование: color <id> <r> <g> <b>");
                return;
            }

            Photo photo = GetPhotoById(id);
            photo.SetImage(filters.ApplyColorFilter(photo.Image, r, g, b));
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Цвет фото {photo.Name} изменён на R:{r} G:{g} B:{b}");
        }

        public void CropPhoto(string[] parts)
        {
            if (parts.Length < 6 ||
                !int.TryParse(parts[1], out int id) ||
                !int.TryParse(parts[2], out int x) ||
                !int.TryParse(parts[3], out int y) ||
                !int.TryParse(parts[4], out int w) ||
                !int.TryParse(parts[5], out int h))
            {
                Console.WriteLine("Использование: crop <id> <x> <y> <w> <h>");
                return;
            }

            Photo photo = GetPhotoById(id);
            editor.Crop(photo, new Rectangle(x, y, w, h));
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Фото {photo.Name} обрезано");
        }

        public void RotatePhoto(string[] parts)
        {
            if (parts.Length < 3 || !int.TryParse(parts[1], out int id) || !float.TryParse(parts[2], out float angle))
            {
                Console.WriteLine("Использование: rotate <id> <угол>");
                return;
            }

            Photo photo = GetPhotoById(id);
            editor.Rotate(photo, angle);
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Фото {photo.Name} повернуто на {angle}°");
        }

        public void ChangePixel(string[] parts)
        {
            if (parts.Length < 7 ||
                !int.TryParse(parts[1], out int id) ||
                !int.TryParse(parts[2], out int x) ||
                !int.TryParse(parts[3], out int y) ||
                !int.TryParse(parts[4], out int r) ||
                !int.TryParse(parts[5], out int g) ||
                !int.TryParse(parts[6], out int b))
            {
                Console.WriteLine("Использование: pixel <id> <x> <y> <r> <g> <b>");
                return;
            }

            Photo photo = GetPhotoById(id);
            editor.ChangePixel(photo, x, y, Color.FromArgb(r, g, b));
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Пиксель ({x},{y}) фото {photo.Name} изменён на R:{r} G:{g} B:{b}");
        }

        public void CreateCollage(string[] parts)
        {
            if (parts.Length < 3)
            {
                Console.WriteLine("Использование: collage <id1,id2,...> <путь_для_сохранения>");
                return;
            }

            string[] idsStr = parts[1].Split(',');
            List<Bitmap> bitmaps = new List<Bitmap>();

            foreach (var s in idsStr)
            {
                if (!int.TryParse(s, out int id))
                {
                    Console.WriteLine($"Неверный ID: {s}");
                    return;
                }

                Photo photo = GetPhotoById(id);
                if (photo.Image == null)
                    photo.LoadImage();
                bitmaps.Add(photo.Image);
            }

            Bitmap collage = collageMaker.CreateCollage(bitmaps);
            string savePath = parts[2];
            collage.Save(savePath);
            Console.WriteLine($"Коллаж создан и сохранён: {savePath}");
        }

        // --- Вспомогательные методы ---
        private Photo GetPhotoById(int id)
        {
            if (id < 1 || id > photos.Count)
                throw new ArgumentOutOfRangeException("Неверный ID фото");
            return photos[id - 1];
        }
    }
}
