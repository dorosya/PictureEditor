using System.Windows.Media.Imaging;

namespace PictureEditor.Interfaces
{
    public interface ICropService
    {
        BitmapSource Crop(BitmapSource source, int x, int y, int width, int height);
    }
}