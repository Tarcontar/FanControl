using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FanControl
{
    public class TrayIcon
    {
        const int WIDTH = 300;
        const int HEIGHT = 300;

        NotifyIcon icon;
        RectangleF rect;

        Bitmap image_bmp;
        Dictionary<Color, Bitmap> colored_images = new Dictionary<Color, Bitmap>();

        public TrayIcon(string image)
        {
            this.icon = new NotifyIcon();
            //this.txt_bmp = new Bitmap(WIDTH, HEIGHT);
            this.image_bmp = new Bitmap(WIDTH, HEIGHT);
            this.rect = new RectangleF(0, 0, WIDTH, HEIGHT);

            this.image_bmp = new Bitmap(new Bitmap(image), WIDTH, HEIGHT);
            //colored_images[Color.Green] = ToColor(image_bmp, Color.Green);
            //colored_images[Color.Orange] = ToColor(image_bmp, Color.Orange);
            //colored_images[Color.Red] = ToColor(image_bmp, Color.Red);

            icon.BalloonTipText = "Hi tip text";
            icon.BalloonTipTitle = "Title";
            icon.ShowBalloonTip(2000);
            icon.Visible = true;
        }

        private Bitmap ToColor(Bitmap bmp, Color color)
        {
            var b = new Bitmap(WIDTH, HEIGHT);
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    if (bmp.GetPixel(x, y).A > 0.9) b.SetPixel(x, y, color);
                }
            }
            return b;
        }

        bool mode = false;

        public void Update(string text, Color color)
        {
            if (mode)
            {
                if (!colored_images.ContainsKey(color))
                {
                    this.colored_images[color] = ToColor(image_bmp, color);
                }
                this.icon.Icon = Icon.FromHandle(this.colored_images[color].GetHicon());
            }
            else
            {
                var brush = new SolidBrush(color);

                var txt_bmp = new Bitmap(WIDTH, HEIGHT);
                var g = Graphics.FromImage(txt_bmp);
                //g.FillRectangle(Brushes.White, 0, 0, WIDTH, HEIGHT);
                //g.FillRectangle(Brushes.Transparent, 0, 0, WIDTH, HEIGHT);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                if (text.Length == 1) text = " " + text;
                g.DrawString(text, new Font("Tahoma", 150), brush, rect);
                g.Dispose();
                this.icon.Icon = Icon.FromHandle(txt_bmp.GetHicon());
            }

            mode = !mode;
        }
    }
}
