using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace Pictopog {
    class ScreenReading {
        static public Bitmap getScreenRegion(int x, int y, int w, int h) {
            Bitmap b = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(x, y, 0, 0, new Size(w, h));
            return b;
        }
    }
}
