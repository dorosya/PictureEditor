using System;
using System.Collections.Generic;
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
            CliCommands commands = new CliCommands(photos);

            commands.PrintHelp();

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
                        case "add": commands.AddPhoto(parts); break;
                        case "list": commands.ListPhotos(); break;
                        case "open": commands.OpenPhoto(parts); break;
                        case "save": StateManager.SaveState(photos); break;
                        case "clear": photos.Clear(); StateManager.ClearState(); break;
                        case "exit": StateManager.SaveState(photos); return;
                        case "brightness": commands.ApplyBrightness(parts); break;
                        case "color": commands.ApplyColor(parts); break;
                        case "crop": commands.CropPhoto(parts); break;
                        case "rotate": commands.RotatePhoto(parts); break;
                        case "pixel": commands.ChangePixel(parts); break;
                        case "collage": commands.CreateCollage(parts); break;
                        case "help": commands.PrintHelp(); break;
                        default: Console.WriteLine("Неизвестная команда. help — список команд"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}
