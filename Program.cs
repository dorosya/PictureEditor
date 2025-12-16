using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using PhotoEditor.Models;
using PhotoEditor.Services;
using PhotoEditor.Utils;

namespace PhotoEditor
{
    class Program
    {
        static Filters filters = new Filters();
        static ImageEditor editor = new ImageEditor();
        static CollageMaker collageMaker = new CollageMaker();

        static void Main()
        {
            Console.WriteLine("=== Photo Editor (CLI) ===");

            List<Photo> photos = StateManager.LoadState();
            PrintHelp();

            while (true)
            {
                Console.Write("\n> ");
                string input = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();

                try
                {
                    switch (command)
                    {
                        case "add":
                            AddPhoto(parts, photos);
                            break;

                        case "list":
                            ListPhotos(photos);
                            break;

                        case "open":
                            OpenPhoto(parts, photos);
                            break;

                        case "save":
                            StateManager.SaveState(photos);
                            break;

                        case "clear":
                            photos.Clear();
                            StateManager.ClearState();
                            break;

                        case "exit":
                            StateManager.SaveState(photos);
                            return;

                        case "brightness":
                            ApplyBrightness(parts, photos);
                            break;

                        case "color":
                            ApplyColor(parts, photos);
                            break;

                        case "crop":
                            CropPhoto(parts, photos);
                            break;

                        case "rotate":
                            RotatePhoto(parts, photos);
                            break;

                        case "pixel":
                            ChangePixel(parts, photos);
                            break;

                        case "collage":
                            CreateCollage(parts, photos);
                            break;

                        case "help":
                            PrintHelp();
                            break;

                        default:
                            Console.WriteLine("Неизвестная команда. help — список команд");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        static void PrintHelp()
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


        static void AddPhoto(string[] parts, List<Photo> photos)
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

        static void ListPhotos(List<Photo> photos)
        {
            if (photos.Count == 0)
            {
                Console.WriteLine("Список фото пуст");
                return;
            }

            for (int i = 0; i < photos.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {photos[i].Name}");
            }
        }

        static void OpenPhoto(string[] parts, List<Photo> photos)
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

        // --- Фильтры ---
        static void ApplyBrightness(string[] parts, List<Photo> photos)
        {
            if (parts.Length < 3 || !int.TryParse(parts[1], out int id) || !float.TryParse(parts[2], out float factor))
            {
                Console.WriteLine("Использование: brightness <id> <factor>");
                return;
            }

            Photo photo = GetPhotoById(id, photos);
            photo.SetImage(filters.ApplyBrightness(photo.Image, factor));
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Яркость фото {photo.Name} изменена на {factor}");
        }

        static void ApplyColor(string[] parts, List<Photo> photos)
        {
            if (parts.Length < 5 || !int.TryParse(parts[1], out int id) ||
                !int.TryParse(parts[2], out int r) ||
                !int.TryParse(parts[3], out int g) ||
                !int.TryParse(parts[4], out int b))
            {
                Console.WriteLine("Использование: color <id> <r> <g> <b>");
                return;
            }

            Photo photo = GetPhotoById(id, photos);
            photo.SetImage(filters.ApplyColorFilter(photo.Image, r, g, b));
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Цвет фото {photo.Name} изменён на R:{r} G:{g} B:{b}");
        }

        // --- ImageEditor ---
        static void CropPhoto(string[] parts, List<Photo> photos)
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

            Photo photo = GetPhotoById(id, photos);
            editor.Crop(photo, new Rectangle(x, y, w, h));
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Фото {photo.Name} обрезано");
        }

        static void RotatePhoto(string[] parts, List<Photo> photos)
        {
            if (parts.Length < 3 || !int.TryParse(parts[1], out int id) || !float.TryParse(parts[2], out float angle))
            {
                Console.WriteLine("Использование: rotate <id> <угол>");
                return;
            }

            Photo photo = GetPhotoById(id, photos);
            editor.Rotate(photo, angle);
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Фото {photo.Name} повернуто на {angle}°");
        }

        static void ChangePixel(string[] parts, List<Photo> photos)
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

            Photo photo = GetPhotoById(id, photos);
            editor.ChangePixel(photo, x, y, Color.FromArgb(r, g, b));
            photo.SaveImage(photo.FilePath);
            Console.WriteLine($"Пиксель ({x},{y}) фото {photo.Name} изменён на R:{r} G:{g} B:{b}");
        }

        // --- CollageMaker ---
        static void CreateCollage(string[] parts, List<Photo> photos)
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

                Photo photo = GetPhotoById(id, photos);
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
        static Photo GetPhotoById(int id, List<Photo> photos)
        {
            if (id < 1 || id > photos.Count)
                throw new ArgumentOutOfRangeException("Неверный ID фото");
            return photos[id - 1];
        }
    }
}
