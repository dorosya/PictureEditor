using System.Collections.Generic;
using System.Drawing;

namespace PhotoEditor.Interfaces
{
    public interface ICollageMaker
    {
        Bitmap CreateCollage(List<Bitmap> images);
    }
}
