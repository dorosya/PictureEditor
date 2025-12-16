using System;
using System.Collections.Generic;
using System.Diagnostics;
using PhotoEditor.Models;
using PhotoEditor.Utils;

namespace PhotoEditor
{
    class Program
    {
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
                string command = parts[0];

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
            Console.WriteLine("add <путь>   — добавить фото");
            Console.WriteLine("list        — список фото");
            Console.WriteLine("open <id>   — открыть фото в Windows");
            Console.WriteLine("save        — сохранить состояние");
            Console.WriteLine("clear       — очистить состояние");
            Console.WriteLine("exit        — выход с сохранением");
        }

        static void AddPhoto(string[] parts, List<Photo> photos)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Использование: add <путь_к_файлу>");
                return;
            }

            Photo photo = new Photo(parts[1]);
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
                Console.WriteLine($"{i + 1}. {photos[i]}");
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

            Process.Start(new ProcessStartInfo
            {
                FileName = photo.FilePath,
                UseShellExecute = true
            });
        }
    }
}
