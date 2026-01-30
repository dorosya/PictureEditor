using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PictureEditor.Interfaces
{
    public interface IPixelEditor
    {
        BitmapSource SetPixel(BitmapSource source, int x, int y, Color color);
    }
}