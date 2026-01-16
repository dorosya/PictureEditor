using System.Windows.Media.Imaging; 

namespace PhotoEditor.Interfaces
{
    public interface IFilter
    { 
        BitmapImage ApplyBrightness(BitmapImage source, float factor);
        BitmapImage ApplyColorFilter(BitmapImage source, int r, int g, int b);
    }
}