using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PhotoEditor
{
    public static class StateManager
    {
        private const string StateFile = "app_state.json";

        public static void SaveState(List<Photo> photos)
        {
            try
            {
                if (photos == null)
                {
                    Console.WriteLine("  Нет данных для сохранения");
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(photos, options);
                File.WriteAllText(StateFile, json);

                Console.WriteLine($" Состояние сохранено: {photos.Count} фото");
                Console.WriteLine($"   Файл: {StateFile}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(" Нет прав для записи файла");
            }
            catch (IOException ex)
            {
                Console.WriteLine($" Ошибка ввода-вывода: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка сохранения: {ex.Message}");
            }
        }

        public static List<Photo> LoadState()
        {
            try
            {
                if (!File.Exists(StateFile))
                {
                    Console.WriteLine("Файл состояния не найден. Создан новый список.");
                    return new List<Photo>();
                }

                string json = File.ReadAllText(StateFile);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine(" Файл состояния пуст");
                    return new List<Photo>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<Photo> photos = JsonSerializer.Deserialize<List<Photo>>(json, options);

                if (photos == null)
                {
                    Console.WriteLine("Ошибка чтения состояния. Создан новый список.");
                    return new List<Photo>();
                }

                Console.WriteLine($"Загружено: {photos.Count} фото");
                return photos;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка формата JSON: {ex.Message}");
                Console.WriteLine("   Создан новый список фото");
                return new List<Photo>();
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Нет прав для чтения файла");
                return new List<Photo>();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка чтения файла: {ex.Message}");
                return new List<Photo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Неизвестная ошибка: {ex.Message}");
                return new List<Photo>();
            }
        }

        public static void DeleteState()
        {
            try
            {
                if (File.Exists(StateFile))
                {
                    File.Delete(StateFile);
                    Console.WriteLine(" Файл состояния удален");
                }
                else
                {
                    Console.WriteLine(" Файл состояния не существует");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка удаления: {ex.Message}");
            }
        }

        public static bool StateExists()
        {
            return File.Exists(StateFile);
        }
    }
}