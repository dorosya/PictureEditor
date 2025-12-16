using System;
using System.Collections.Generic;

namespace PhotoEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Photo Editor ===");

            try
            {
                // Загрузка состояния
                Console.WriteLine("Загрузка состояния...");
                List<Photo> photos = StateManager.LoadState();

                if (photos.Count == 0)
                {
                    Console.WriteLine("Список фото пуст. Добавьте фото командой 'add'");
                }
                else
                {
                    Console.WriteLine($"Готово к работе. Фото в памяти: {photos.Count}");
                }

                // Меню команд
                Console.WriteLine("\n=== Команды ===");
                Console.WriteLine("add [имя]  - добавить фото");
                Console.WriteLine("list - список фото");
                Console.WriteLine("save - сохранить состояние");
                Console.WriteLine("clear - очистить состояние");
                Console.WriteLine("exit - выход с сохранением");
                Console.WriteLine("================\n");

                while (true)
                {
                    Console.Write("> ");
                    string input = Console.ReadLine()?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    if (input == "exit")
                    {
                        Console.WriteLine("\nСохранение состояния...");
                        StateManager.SaveState(photos);
                        Console.WriteLine("Выход...");
                        break;
                    }
                    else if (input == "list")
                    {
                        if (photos.Count == 0)
                        {
                            Console.WriteLine("Список фото пуст");
                        }
                        else
                        {
                            Console.WriteLine($"Всего фото: {photos.Count}");
                            for (int i = 0; i < photos.Count; i++)
                            {
                                Console.Write($"{i + 1}. ");
                                photos[i].Display();
                            }
                        }
                    }
                    else if (input == "save")
                    {
                        StateManager.SaveState(photos);
                    }
                    else if (input == "clear")
                    {
                        Console.Write("Удалить все фото? (y/n): ");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            photos.Clear();
                            StateManager.DeleteState();
                            Console.WriteLine("Все фото удалены");
                        }
                    }
                    else if (input.StartsWith("add "))
                    {
                        string name = input.Substring(4).Trim();
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            Console.WriteLine("Укажите имя файла: add example.jpg");
                        }
                        else
                        {
                            photos.Add(new Photo(name));
                            Console.WriteLine($"Добавлено: {name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неизвестная команда. Доступно: add, list, save, clear, exit");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Критическая ошибка: {ex.Message}");
                Console.WriteLine("Программа завершена");
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}