using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class ImageExtention
    {
        public static Image DiscretImage(this Image image, int width, int height)
        {
            Bitmap b = new Bitmap(image, new Size(width, height));
            int[,] point = new int[b.Width, b.Height];

            for (int n = 0; n < point.GetLength(0); n++)
                for (int m = 0; m < point.GetLength(1); m++)
                    point[n, m] = (b.GetPixel(n, m).R + b.GetPixel(n, m).G + b.GetPixel(n, m).B) / 3;

            Bitmap bitmap = new Bitmap(b);
            for (int x = 0; x < point.GetLength(0); x += 1)
                for (int y = 0; y < point.GetLength(1); y += 1)
                    if (point[x, y] > 128)
                        bitmap.SetPixel(x, y, Color.White);
                    else
                        bitmap.SetPixel(x, y, Color.Black);

            return bitmap;
        }
    }
}
