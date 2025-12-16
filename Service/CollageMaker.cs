using System;
using System.Collections.Generic;
using System.Drawing;
using PhotoEditor.Interfaces;

namespace PhotoEditor.Services
{
    public class CollageMaker : ICollageMaker
    {
        public Bitmap CreateCollage(List<Bitmap> images)
        {
            if (images == null || images.Count == 0)
                throw new ArgumentException("Список изображений пуст");

            int count = images.Count;

            // Размер сетки (например, 4 картинки → 2x2)
            int gridSize = (int)Math.Ceiling(Math.Sqrt(count));

            // Размер ячейки — по максимальному изображению
            int cellWidth = 0;
            int cellHeight = 0;

            foreach (var img in images)
            {
                cellWidth = Math.Max(cellWidth, img.Width);
                cellHeight = Math.Max(cellHeight, img.Height);
            }

            int collageWidth = gridSize * cellWidth;
            int collageHeight = gridSize * cellHeight;

            Bitmap collage = new Bitmap(collageWidth, collageHeight);

            using (Graphics g = Graphics.FromImage(collage))
            {
                g.Clear(Color.White);

                for (int i = 0; i < count; i++)
                {
                    int row = i / gridSize;
                    int col = i % gridSize;

                    int x = col * cellWidth;
                    int y = row * cellHeight;

                    g.DrawImage(images[i], x, y, cellWidth, cellHeight);
                }
            }

            return collage;
        }
    }
}
