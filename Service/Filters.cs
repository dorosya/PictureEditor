using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace PictureEditor.Service
{
    public class Filters : Interfaces.IFilter
    {
        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (MemoryStream memory = new MemoryStream())
            {
                encoder.Save(memory);
                return new Bitmap(memory);
            }
        }

      
        public BitmapImage ApplyBrightness(BitmapImage source, float factor)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap bitmap = BitmapImageToBitmap(source);
            Bitmap result = ApplyBrightnessOld(bitmap, factor);
            return BitmapToBitmapImage(result);
        }

    
        public BitmapImage ApplyColorFilter(BitmapImage source, int rOffset, int gOffset, int bOffset)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap bitmap = BitmapImageToBitmap(source);
            Bitmap result = ApplyColorFilterOld(bitmap, rOffset, gOffset, bOffset);
            return BitmapToBitmapImage(result);
        }

       
        private Bitmap ApplyBrightnessOld(Bitmap source, float factor)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    
                    System.Drawing.Color original = source.GetPixel(x, y);

                    int r = Clamp((int)(original.R * factor));
                    int g = Clamp((int)(original.G * factor));
                    int b = Clamp((int)(original.B * factor));

                    result.SetPixel(x, y, System.Drawing.Color.FromArgb(r, g, b));
                }
            }

            return result;
        }

       
        private Bitmap ApplyColorFilterOld(Bitmap source, int rOffset, int gOffset, int bOffset)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    
                    System.Drawing.Color original = source.GetPixel(x, y);

                    int r = Clamp(original.R + rOffset);
                    int g = Clamp(original.G + gOffset);
                    int b = Clamp(original.B + bOffset);

                    result.SetPixel(x, y, System.Drawing.Color.FromArgb(r, g, b));
                }
            }

            return result;
        }

        
        private int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }
    }
}