using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;

namespace PhotoEditor.Models
{
    [Serializable]
    public class Photo
    {
        public string FilePath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public Bitmap? Image { get; private set; }

        public Photo() { }

        public Photo(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл изображения не найден", filePath);

            FilePath = Path.GetFullPath(filePath);
            Name = Path.GetFileName(filePath);
        }

        public void LoadImage()
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("Файл изображения не найден", FilePath);

            Image?.Dispose();
            Image = new Bitmap(FilePath);
        }

        public void SaveImage(string outputPath)
        {
            if (Image == null)
                throw new InvalidOperationException("Изображение не загружено");

            Image.Save(outputPath);
            FilePath = Path.GetFullPath(outputPath);
            Name = Path.GetFileName(outputPath);
        }

        public void UnloadImage()
        {
            Image?.Dispose();
            Image = null;
        }

        public override string ToString()
        {
            return $"{Name} ({FilePath})";
        }
        public void SetImage(Bitmap bitmap)
        {
            Image?.Dispose();
            Image = bitmap;
        }
    }
}
