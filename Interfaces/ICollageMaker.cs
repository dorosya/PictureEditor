using System.Collections.Generic;
using System.Windows.Media.Imaging;  

namespace PictureEditor.Interfaces
{
    public interface ICollageMaker
    {
        BitmapImage CreateCollage(List<BitmapImage> images);
    }
}