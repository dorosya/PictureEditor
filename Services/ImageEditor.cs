using System;
using System.Drawing;

namespace PhotoEditor
{
    /// Класс для редактирования изображений
    public class ImageEditor
    {
        /// Загрузка изображения в объект Photo
        public void LoadImage(Photo photo)
        {
            if (!File.Exists(photo.FilePath))
                throw new FileNotFoundException("Файл изображения не найден");

            photo.Image = new Bitmap(photo.FilePath);
        }

        /// Обрезка изображения
        public void Crop(Photo photo, Rectangle area)
        {
            if (photo.Image == null)
                throw new InvalidOperationException("Изображение не загружено");

            Bitmap source = photo.Image;
            Bitmap cropped = new Bitmap(area.Width, area.Height);

            using (Graphics g = Graphics.FromImage(cropped))
            {
                g.DrawImage(
                    source,
                    new Rectangle(0, 0, area.Width, area.Height),
                    area,
                    GraphicsUnit.Pixel);
            }

            photo.Image = cropped;
        }


        /// Поворот изображения на угол
        public void Rotate(Photo photo, float angle)
        {
            if (photo.Image == null)
                throw new InvalidOperationException("Изображение не загружено");

            Bitmap source = photo.Image;
            Bitmap rotated = new Bitmap(source.Width, source.Height);

            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.TranslateTransform(source.Width / 2f, source.Height / 2f);
                g.RotateTransform(angle);
                g.TranslateTransform(-source.Width / 2f, -source.Height / 2f);
                g.DrawImage(source, new Point(0, 0));
            }

            photo.Image = rotated;
        }


        /// Изменение одного пикселя
        public void ChangePixel(Photo photo, int x, int y, Color color)
        {
            if (photo.Image == null)
                throw new InvalidOperationException("Изображение не загружено");

            if (x < 0 || y < 0 || x >= photo.Image.Width || y >= photo.Image.Height)
                throw new ArgumentOutOfRangeException();

            photo.Image.SetPixel(x, y, color);
        }
    }
}
