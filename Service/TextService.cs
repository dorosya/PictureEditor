using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictureEditor.Interfaces;

namespace PictureEditor.Service
{
    public class TextService : ITextService
    {
        public BitmapSource AddText(BitmapSource source, string text, int x, int y, int fontSize, Color color)
        {
            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));

                FormattedText formattedText = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    fontSize,
                    new SolidColorBrush(color),
                    1.0);

                dc.DrawText(formattedText, new Point(x, y));
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap(
                source.PixelWidth,
                source.PixelHeight,
                source.DpiX,
                source.DpiY,
                PixelFormats.Pbgra32);

            bmp.Render(visual);
            return bmp;
        }
    }
}