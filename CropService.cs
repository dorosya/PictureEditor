using System;
using System.Windows.Media.Imaging;
using PictureEditor.Interfaces;

namespace PictureEditor.Service
{
    public class CropService : ICropService
    {
        public BitmapSource Crop(BitmapSource source, int x, int y, int width, int height)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (x < 0 || y < 0 || width <= 0 || height <= 0 ||
                x + width > source.PixelWidth ||
                y + height > source.PixelHeight)
                throw new ArgumentException("Некорректные параметры обрезки");

            return new CroppedBitmap(source, new System.Windows.Int32Rect(x, y, width, height));
        }
    }
}