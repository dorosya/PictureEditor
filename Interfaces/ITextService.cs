using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PictureEditor.Interfaces
{
    public interface ITextService
    {
        BitmapSource AddText(BitmapSource source, string text, int x, int y, int fontSize, Color color);
    }
}