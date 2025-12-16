using System;
using System.Drawing;
using PhotoEditor.Interfaces;

namespace PhotoEditor.Services
{
    public class Filters : IFilter
    {
        public Bitmap ApplyBrightness(Bitmap source, float factor)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color original = source.GetPixel(x, y);

                    int r = Clamp((int)(original.R * factor));
                    int g = Clamp((int)(original.G * factor));
                    int b = Clamp((int)(original.B * factor));

                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return result;
        }

        public Bitmap ApplyColorFilter(Bitmap source, int rOffset, int gOffset, int bOffset)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color original = source.GetPixel(x, y);

                    int r = Clamp(original.R + rOffset);
                    int g = Clamp(original.G + gOffset);
                    int b = Clamp(original.B + bOffset);

                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
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
