using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictureEditor.Interfaces;

namespace PictureEditor.Service
{
    public class PixelEditor : IPixelEditor
    {
        public BitmapSource SetPixel(BitmapSource source, int x, int y, Color color)
        {
            WriteableBitmap wb = new WriteableBitmap(source);

            byte[] pixel = { color.B, color.G, color.R, color.A };

            wb.WritePixels(
                new System.Windows.Int32Rect(x, y, 1, 1),
                pixel,
                4,
                0);

            return wb;
        }
    }
}