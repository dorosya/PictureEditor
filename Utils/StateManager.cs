using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PhotoEditor.Models;

namespace PhotoEditor.Utils
{
    public static class StateManager
    {
        private const string StateFile = "app_state.json";

        public static void SaveState(List<Photo> photos)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(photos, options);
            File.WriteAllText(StateFile, json);

            Console.WriteLine($"Состояние сохранено ({photos.Count} фото)");
        }

        public static List<Photo> LoadState()
        {
            if (!File.Exists(StateFile))
            {
                Console.WriteLine("Сохранённое состояние не найдено");
                return new List<Photo>();
            }

            string json = File.ReadAllText(StateFile);

            var photos = JsonSerializer.Deserialize<List<Photo>>(json);

            Console.WriteLine($"Загружено фото: {photos?.Count ?? 0}");
            return photos ?? new List<Photo>();
        }

        public static void ClearState()
        {
            if (File.Exists(StateFile))
            {
                File.Delete(StateFile);
                Console.WriteLine("Состояние очищено");
            }
        }
    }
}
