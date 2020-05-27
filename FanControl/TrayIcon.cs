using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FanControl
{
    public class TrayIcon
    {

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        const int WIDTH = 300;
        const int HEIGHT = 300;

        NotifyIcon icon;
        RectangleF rect;

        Bitmap txt_bmp;
        Bitmap image_bmp;
        Dictionary<Color, Icon> colored_icons = new Dictionary<Color, Icon>();

        Icon textIcon;

        public TrayIcon(string image)
        {
            this.icon = new NotifyIcon();
            this.image_bmp = new Bitmap(WIDTH, HEIGHT);
            this.txt_bmp = new Bitmap(WIDTH, HEIGHT);
            this.rect = new RectangleF(0, 0, WIDTH, HEIGHT);

            this.image_bmp = new Bitmap(new Bitmap(image), WIDTH, HEIGHT);

            

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

        public void Update(string text, Color color, string info_text = "")
        {
            icon.Text = info_text;

            if (mode)
            {

                if (!colored_icons.ContainsKey(color))
                {
                    this.colored_icons[color] = Icon.FromHandle(ToColor(image_bmp, color).GetHicon());
                }
                this.icon.Icon = this.colored_icons[color];
            }
            else
            {
                var brush = new SolidBrush(color);

                var g = Graphics.FromImage(this.txt_bmp);
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                if (text.Length == 1) text = " " + text;
                g.DrawString(text, new Font("Tahoma", 150), brush, rect);
                g.Dispose();
                var ico = Icon.FromHandle(this.txt_bmp.GetHicon());

                this.icon.Icon = ico;

                DestroyIcon(ico.Handle);
            }

            mode = !mode;
        }
    }
}
