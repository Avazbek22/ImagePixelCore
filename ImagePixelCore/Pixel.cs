using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePixelCore
{
    public class Pixel
    {
        public Pixel(Point point, Color color)
        {
            Point = point;
            Color = color;
        }

        public Point Point { get; set; }
        public Color Color { get; set; }
    }
}
