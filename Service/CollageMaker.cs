using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;
using PhotoEditor.Interfaces;

namespace PhotoEditor.Services
{
    public class CollageMaker : ICollageMaker
    {
        public BitmapImage CreateCollage(List<BitmapImage> images)
        {
            if (images == null || images.Count == 0)
                throw new ArgumentException("Список изображений пуст");

            int count = images.Count;
            int gridSize = (int)Math.Ceiling(Math.Sqrt(count));
            int cellWidth = 0;
            int cellHeight = 0;

            foreach (var img in images)
            {
                cellWidth = Math.Max(cellWidth, img.PixelWidth);
                cellHeight = Math.Max(cellHeight, img.PixelHeight);
            }

            int collageWidth = gridSize * cellWidth;
            int collageHeight = gridSize * cellHeight;

          
            var renderBitmap = new RenderTargetBitmap(
                collageWidth, collageHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);

            var drawingVisual = new System.Windows.Media.DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.White,
                    null,
                    new System.Windows.Rect(0, 0, collageWidth, collageHeight));

                for (int i = 0; i < count; i++)
                {
                    int row = i / gridSize;
                    int col = i % gridSize;

                    double x = col * cellWidth;
                    double y = row * cellHeight;

                    drawingContext.DrawImage(
                        images[i],
                        new System.Windows.Rect(x, y, cellWidth, cellHeight));
                }
            }

            renderBitmap.Render(drawingVisual);
            renderBitmap.Freeze();

        
            var bitmapImage = new BitmapImage();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }
    }
}