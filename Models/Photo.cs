using System;
using System.Text.Json;
using System.Drawing;
using System.Text.Json.Serialization;
using System.IO;

namespace PhotoEditor
{
    [Serializable]
    public class Photo
    {
        public string FilePath { get; set; } = "";
        public string Name { get; set; } = "";

        
        [JsonIgnore] 
        public Bitmap? Image { get; set; }

        public Photo() { }

        public Photo(string path)
        {
            FilePath = path;
            Name = Path.GetFileName(path);
        }

        
        public void SaveToFile(string filePath)
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(filePath, json);
            Console.WriteLine($"Фото сохранено в JSON: {filePath}");
        }

       
        public static Photo LoadFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            Photo photo = JsonSerializer.Deserialize<Photo>(json)!;
            Console.WriteLine($"Фото загружено из JSON: {photo.Name}");
            return photo;
        }

        public void Display()
        {
            Console.WriteLine($" {Name}");
            if (File.Exists(FilePath))
            {
                Console.WriteLine($"  Путь: {FilePath}");
            }
        }
    }
}