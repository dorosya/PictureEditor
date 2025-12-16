using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
            Console.WriteLine("\nКоманды:");
            Console.WriteLine("add <путь>           — добавить фото");
            Console.WriteLine("list                 — список фото");
            Console.WriteLine("open <id>            — открыть фото в Windows");
            Console.WriteLine("save                 — сохранить состояние");
            Console.WriteLine("clear                — очистить состояние");
            Console.WriteLine("brightness <id> <f>  — изменить яркость (f = 0.5..2.0)");
            Console.WriteLine("color <id> <r> <g> <b> — изменить цвет (0..255)");
            Console.WriteLine("exit                 — выход с сохранением");
        }

        static void AddPhoto(string[] parts, List<Photo> photos)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Использование: add <путь_к_файлу>");
                return;
            }

            Photo photo = new Photo(parts[1]);
            photo.LoadImage(); // сразу загружаем изображение
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

        static void ApplyBrightness(string[] parts, List<Photo> photos)
        {
            if (parts.Length < 3 || !int.TryParse(parts[1], out int id) || !float.TryParse(parts[2], out float factor))
            {
                Console.WriteLine("Использование: brightness <id> <factor>");
                return;
            }

            if (id < 1 || id > photos.Count)
            {
                Console.WriteLine("Неверный ID");
                return;
            }

            Photo photo = photos[id - 1];
            if (photo.Image == null)
                photo.LoadImage();

            // Применяем фильтр и сохраняем результат в файл
            photo.SetImage(filters.ApplyBrightness(photo.Image, factor));
            photo.SaveImage(photo.FilePath);

            Console.WriteLine($"Яркость фото {photo.Name} изменена на {factor} и сохранена");
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

            if (id < 1 || id > photos.Count)
            {
                Console.WriteLine("Неверный ID");
                return;
            }

            Photo photo = photos[id - 1];
            if (photo.Image == null)
                photo.LoadImage();

            // Применяем фильтр и сохраняем результат в файл
            photo.SetImage(filters.ApplyColorFilter(photo.Image, r, g, b));
            photo.SaveImage(photo.FilePath);

            Console.WriteLine($"Цветовое преобразование фото {photo.Name} применено и сохранено (R:{r} G:{g} B:{b})");
        }
    }
}
