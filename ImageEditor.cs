using System;
using System.Drawing;

namespace PhotoEditor
{
    /// <summary>
    /// Класс редактирования изображений
    /// </summary>
    public class ImageEditor
    {
        private readonly Services _services = new Services();

        /// <summary>
        /// Обрезка изображения
        /// </summary>
        public void Crop(Photo photo, Rectangle area)
        {
            _services.EnsureImageLoaded(photo);

            Bitmap source = photo.Image!;
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

        /// <summary>
        /// Поворот изображения на угол
        /// </summary>
        public void Rotate(Photo photo, float angle)
        {
            _services.EnsureImageLoaded(photo);

            Bitmap source = photo.Image!;
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

        /// <summary>
        /// Изменение одного пикселя
        /// </summary>
        public void ChangePixel(Photo photo, int x, int y, Color color)
        {
            _services.EnsureImageLoaded(photo);

            if (x < 0 || y < 0 ||
                x >= photo.Image!.Width || y >= photo.Image.Height)
                throw new ArgumentOutOfRangeException();

            photo.Image.SetPixel(x, y, color);
        }
    }
}
