using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PictureEditor.Models;
using System.Windows.Media.Imaging;
using PictureEditor.Interfaces;

namespace PictureEditor.Service
{
    public class ImageEditor 
    {
        private BitmapSource? _currentImage;

        private readonly ICropService _cropService;
        private readonly ITextService _textService;
        private readonly IPixelEditor _pixelEditor;

        public BitmapSource? CurrentImage => _currentImage;

        public ImageEditor()
        {
            _cropService = new CropService();
            _textService = new TextService();
            _pixelEditor = new PixelEditor();
        }

        public void LoadImage(BitmapSource image)
        {
            _currentImage = image ?? throw new ArgumentNullException(nameof(image));
        }

        // ========= НОВОЕ =========

        public void CropImage(int x, int y, int width, int height)
        {
            _currentImage = _cropService.Crop(_currentImage, x, y, width, height);
        }

        public void AddText(string text, int x, int y, int fontSize)
        {
            _currentImage = _textService.AddText(
                _currentImage,
                text,
                x,
                y,
                fontSize,
                System.Windows.Media.Colors.Red);
        }

        public void SetPixel(int x, int y)
        {
            _currentImage = _pixelEditor.SetPixel(
                _currentImage,
                x,
                y,
                System.Windows.Media.Colors.Blue);
        }

        public void Rotate(Photo photo, float angle)
        {
            if (photo == null) throw new ArgumentNullException(nameof(photo));
            if (string.IsNullOrWhiteSpace(photo.FilePath) || !File.Exists(photo.FilePath))
                throw new FileNotFoundException("Файл изображения не найден", photo.FilePath);

            using var src = new Bitmap(photo.FilePath);

            // Вычисляем размеры нового холста, чтобы изображение не обрезалось при произвольном угле.
            double rad = angle * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(rad));
            double sin = Math.Abs(Math.Sin(rad));

            int newW = (int)Math.Ceiling(src.Width * cos + src.Height * sin);
            int newH = (int)Math.Ceiling(src.Width * sin + src.Height * cos);

            using var dest = new Bitmap(newW, newH);
            dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            using (var g = Graphics.FromImage(dest))
            {
                g.Clear(Color.Transparent);
                g.TranslateTransform(newW / 2f, newH / 2f);
                g.RotateTransform(angle);
                g.TranslateTransform(-src.Width / 2f, -src.Height / 2f);
                g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height));
            }

            // Сохраняем в исходный файл (по расширению).
            var ext = Path.GetExtension(photo.FilePath).ToLowerInvariant();
            ImageFormat format = ext switch
            {
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".bmp" => ImageFormat.Bmp,
                _ => ImageFormat.Png
            };

            // Чтобы избежать частичных записей — пишем во временный файл и заменяем.
            var tmp = photo.FilePath + ".tmp";
            dest.Save(tmp, format);

            File.Copy(tmp, photo.FilePath, overwrite: true);
            File.Delete(tmp);
        }

    }
}