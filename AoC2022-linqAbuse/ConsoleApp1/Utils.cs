using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal static class Utils
    {
        public struct Point
        {
            public int x = 0;
            public int y = 0;

            public Point(int _x, int _y)
            {
                x = _x;
                y = _y;
            }

            public static Point operator +(Point x, Point y)
            {
                return new Point(x.x + y.x, x.y + y.y);
            }

            public static Point operator -(Point x, Point y)
            {
                return new Point(x.x - y.x, x.y - y.y);
            }

            public static bool operator ==(Point x, Point y)
            {
                return x.x == y.x && x.y == y.y;
            }

            public static bool operator !=(Point x, Point y)
            {
                return !(x == y);
            }

            public override string ToString()
            {
                return "(" + x + "," + y + ")";
            }
        }
    }
}
