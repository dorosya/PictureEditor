using System;
using System.Drawing;
using System.IO;

namespace PhotoEditor
{
    /// Сервисные операции для работы с Photo
    public class Services
    {
        /// Загрузка изображения из файла в объект Photo
        public void LoadImage(Photo photo)
        {
            if (!File.Exists(photo.FilePath))
                throw new FileNotFoundException("Файл изображения не найден");

            photo.Image = new Bitmap(photo.FilePath);
        }

        /// Проверка, что изображение загружено
        public void EnsureImageLoaded(Photo photo)
        {
            if (photo.Image == null)
                throw new InvalidOperationException("Изображение не загружено");
        }

        /// Сохранение изображения в файл
        public void SaveImage(Photo photo, string path)
        {
            EnsureImageLoaded(photo);
            photo.Image!.Save(path);
        }
    }
}
