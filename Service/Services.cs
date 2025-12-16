using System;
using System.Drawing;
using System.IO;
using PhotoEditor.Models;

namespace PhotoEditor.Services
{
    public class Service
    {
        public void LoadImage(Photo photo)
        {
            if (!File.Exists(photo.FilePath))
                throw new FileNotFoundException("Файл изображения не найден", photo.FilePath);

            photo.LoadImage(); // Используем метод самого Photo
        }

        /// Проверяет, что изображение загружено
        public void EnsureImageLoaded(Photo photo)
        {
            if (photo.Image == null)
                throw new InvalidOperationException("Изображение не загружено");
        }

        /// Сохраняет текущее изображение Photo на диск
        public void SaveImage(Photo photo, string path)
        {
            EnsureImageLoaded(photo);
            photo.SaveImage(path); // Используем метод Photo
        }
    }
}
