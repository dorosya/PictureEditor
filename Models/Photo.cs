using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using PictureEditor.Interfaces;
using System.Windows;
using System.Windows.Media;

namespace PictureEditor.Models
{
    [Serializable]
    public class Photo : IPhoto
    {
        public string FilePath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public Bitmap? Image { get; private set; }

        [JsonIgnore]
        public BitmapImage? BitmapImage { get; private set; }

        public Photo() { }

        public Photo(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл изображения не найден", filePath);

            FilePath = Path.GetFullPath(filePath);
            Name = Path.GetFileName(filePath);
        }

        public void LoadImage()
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("Файл изображения не найден", FilePath);

            Image?.Dispose();
            Image = new Bitmap(FilePath);
            UpdateBitmapImage();
        }

       
        public void UpdateBitmapImage()
        {
            if (Image == null)
            {
                BitmapImage = null;
                return;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                BitmapImage = bitmapImage;
            }
        }

        public void SaveImage(string outputPath)
        {
            if (Image == null)
                throw new InvalidOperationException("Изображение не загружено");

            Image.Save(outputPath);
            FilePath = Path.GetFullPath(outputPath);
            Name = Path.GetFileName(outputPath);
        }

        public void UnloadImage()
        {
            Image?.Dispose();
            Image = null;
            BitmapImage = null;
        }

        public void SetImage(Bitmap bitmap)
        {
            Image?.Dispose();
            Image = bitmap;
            UpdateBitmapImage();
        }

        public void SetImage(BitmapImage bitmapImage)
        {
            if (bitmapImage == null)
            {
                Image = null;
                BitmapImage = null;
                return;
            }

    
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                ms.Position = 0;
                Image = new Bitmap(ms);
            }

            BitmapImage = bitmapImage;
        }

        /// <summary>
        /// Устанавливает изображение из WPF <see cref="BitmapSource"/> (CroppedBitmap/RenderTargetBitmap/WriteableBitmap и т.п.).
        /// Нужен, чтобы без ошибок компиляции принимать результат WPF-операций редактирования.
        /// </summary>
        public void SetImage(BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
            {
                Image?.Dispose();
                Image = null;
                BitmapImage = null;
                return;
            }

            // Кодируем BitmapSource в PNG, чтобы получить и Bitmap (System.Drawing), и BitmapImage (WPF)
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                ms.Position = 0;

                Image?.Dispose();
                Image = new Bitmap(ms);

                ms.Position = 0;
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();

                BitmapImage = bi;
            }
        }

        public override string ToString()
        {
            return $"{Name} ({FilePath})";
        }
    }
}
