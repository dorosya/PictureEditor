using System.Windows.Media.Imaging; 

namespace PictureEditor.Interfaces
{
    public interface IFilter
    { 
        BitmapImage ApplyBrightness(BitmapImage source, float factor);
        BitmapImage ApplyColorFilter(BitmapImage source, int r, int g, int b);
    }
}