using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal static class Utils
    {
        public struct PointFloat
        {
            public float X = 0;
            public float Y = 0;

            public PointFloat(float _x, float _y)
            {
                X = _x;
                Y = _y;
            }

            public static PointFloat operator +(PointFloat x, PointFloat y)
            {
                return new PointFloat(x.X + y.X, x.Y + y.Y);
            }

            public static PointFloat operator -(PointFloat x, PointFloat y)
            {
                return new PointFloat(x.X - y.X, x.Y - y.Y);
            }

            public static bool operator ==(PointFloat x, PointFloat y)
            {
                return x.X == y.X && x.Y == y.Y;
            }

            public static bool operator !=(PointFloat x, PointFloat y)
            {
                return !(x == y);
            }

            public override string ToString()
            {
                return "(" + X + "," + Y + ")";
            }
        }

        public struct Point64
        {
            public Int64 X = 0;
            public Int64 Y = 0;

            public Point64(Int64 _x, Int64 _y)
            {
                X = _x;
                Y = _y;
            }

            public static Point64 operator +(Point64 x, Point64 y)
            {
                return new Point64(x.X + y.X, x.Y + y.Y);
            }

            public static Point64 operator -(Point64 x, Point64 y)
            {
                return new Point64(x.X - y.X, x.Y - y.Y);
            }

            public static bool operator ==(Point64 x, Point64 y)
            {
                return x.X == y.X && x.Y == y.Y;
            }

            public static bool operator !=(Point64 x, Point64 y)
            {
                return !(x == y);
            }

            public override string ToString()
            {
                return "(" + X + "," + Y + ")";
            }
        }

        public struct Point: IComparable
        {
            public int X = 0;
            public int Y = 0;

            public Point(int _x, int _y)
            {
                X = _x;
                Y = _y;
            }

            public static Point operator +(Point x, Point y)
            {
                return new Point(x.X + y.X, x.Y + y.Y);
            }

            public static Point operator -(Point x, Point y)
            {
                return new Point(x.X - y.X, x.Y - y.Y);
            }

            public static bool operator ==(Point x, Point y)
            {
                return x.X == y.X && x.Y == y.Y;
            }

            public static bool operator !=(Point x, Point y)
            {
                return !(x == y);
            }

            public override string ToString()
            {
                return "(" + X + "," + Y + ")";
            }

            public override int GetHashCode()
            {
                return (X * 1000000 + Y);
            }

            int IComparable.CompareTo(object? obj)
            {
                if (obj == null) return 1;

                Point? op = (Point)obj;
                if (op.HasValue)
                {
                    int comp = this.X.CompareTo(op.Value.X);
                    if (comp == 0)
                        return this.Y.CompareTo(op.Value.Y);
                    return comp;
                }
                else
                    throw new ArgumentException("Object is not a Point");
            }
        }
    }
}
