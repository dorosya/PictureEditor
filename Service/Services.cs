using System;
using System.Drawing;
using System.IO;
using PhotoEditor.Models;

namespace PhotoEditor.Services
{
    public class Service
    {
        /// <summary>
        /// Загружает изображение в объект Photo
        /// </summary>
        public void LoadImage(Photo photo)
        {
            if (!File.Exists(photo.FilePath))
                throw new FileNotFoundException("Файл изображения не найден", photo.FilePath);

            photo.LoadImage(); // Используем метод самого Photo
        }

        /// <summary>
        /// Проверяет, что изображение загружено
        /// </summary>
        public void EnsureImageLoaded(Photo photo)
        {
            if (photo.Image == null)
                throw new InvalidOperationException("Изображение не загружено");
        }

        /// <summary>
        /// Сохраняет текущее изображение Photo на диск
        /// </summary>
        public void SaveImage(Photo photo, string path)
        {
            EnsureImageLoaded(photo);
            photo.SaveImage(path); // Используем метод Photo
        }
    }
}
