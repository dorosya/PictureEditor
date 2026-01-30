using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PictureEditor.Models;

namespace PictureEditor.Utils
{
    public static class StateManager
    {
        private const string StateFile = "app_state.json";

        public static void SaveState(IEnumerable<Photo> photos)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                var photoData = new List<PhotoData>();
                foreach (var photo in photos)
                {
                    photoData.Add(new PhotoData
                    {
                        FilePath = photo.FilePath,
                        Name = photo.Name
                    });
                }

                string json = JsonSerializer.Serialize(photoData, options);
                File.WriteAllText(StateFile, json);
            }
            catch { /* Игнорируем ошибки сохранения */ }
        }

        public static List<Photo> LoadState()
        {
            if (!File.Exists(StateFile))
                return new List<Photo>();

            try
            {
                string json = File.ReadAllText(StateFile);
                var photoData = JsonSerializer.Deserialize<List<PhotoData>>(json);

                var photos = new List<Photo>();
                foreach (var data in photoData ?? new List<PhotoData>())
                {
                    if (File.Exists(data.FilePath))
                    {
                        photos.Add(new Photo(data.FilePath));
                    }
                }

                return photos;
            }
            catch
            {
                return new List<Photo>();
            }
        }

        private class PhotoData
        {
            public string FilePath { get; set; }
            public string Name { get; set; }
        }
    }
}