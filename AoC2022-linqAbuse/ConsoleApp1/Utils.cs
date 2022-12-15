using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal static class Utils
    {
        public struct Point64
        {
            public Int64 x = 0;
            public Int64 y = 0;

            public Point64(Int64 _x, Int64 _y)
            {
                x = _x;
                y = _y;
            }

            public static Point64 operator +(Point64 x, Point64 y)
            {
                return new Point64(x.x + y.x, x.y + y.y);
            }

            public static Point64 operator -(Point64 x, Point64 y)
            {
                return new Point64(x.x - y.x, x.y - y.y);
            }

            public static bool operator ==(Point64 x, Point64 y)
            {
                return x.x == y.x && x.y == y.y;
            }

            public static bool operator !=(Point64 x, Point64 y)
            {
                return !(x == y);
            }

            public override string ToString()
            {
                return "(" + x + "," + y + ")";
            }
        }

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
