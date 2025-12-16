using System.Drawing;

namespace PhotoEditor.Interfaces
{
    public interface IFilter
    {
        Bitmap ApplyBrightness(Bitmap source, float factor);
        Bitmap ApplyColorFilter(Bitmap source, int r, int g, int b);
    }
}
