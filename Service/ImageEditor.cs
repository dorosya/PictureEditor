using System;
using System.Drawing;
using PhotoEditor.Models;

namespace PhotoEditor.Services
{
    public class ImageEditor
    {
        public void Crop(Photo photo, Rectangle area)
        {
            EnsureLoaded(photo);

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

            photo.SetImage(cropped);
        }

        public void Rotate(Photo photo, float angle)
        {
            EnsureLoaded(photo);

            Bitmap source = photo.Image!;
            Bitmap rotated = new Bitmap(source.Width, source.Height);

            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.TranslateTransform(source.Width / 2f, source.Height / 2f);
                g.RotateTransform(angle);
                g.TranslateTransform(-source.Width / 2f, -source.Height / 2f);
                g.DrawImage(source, new Point(0, 0));
            }

            photo.SetImage(rotated);
        }

        public void ChangePixel(Photo photo, int x, int y, Color color)
        {
            EnsureLoaded(photo);

            if (x < 0 || y < 0 ||
                x >= photo.Image!.Width || y >= photo.Image.Height)
                throw new ArgumentOutOfRangeException();

            photo.Image.SetPixel(x, y, color);
        }

        private void EnsureLoaded(Photo photo)
        {
            if (photo.Image == null)
                throw new InvalidOperationException("Изображение не загружено");
        }
    }
}
