using System.Drawing;

namespace PhotoEditor.Interfaces
{
    public interface IPhoto
    {
        string FilePath { get; }
        string Name { get; }
        Bitmap? Image { get; }

        void LoadImage();
        void SaveImage(string outputPath);
        void UnloadImage();
        void SetImage(Bitmap bitmap);
    }
}
