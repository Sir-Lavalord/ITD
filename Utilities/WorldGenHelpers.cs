using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using System;
namespace ITD.Utilities
{
    public static class WorldGenHelpers
    {

    }
    public static class ITDShapes
    {
        public abstract class  ITDShape
        {
            public abstract Rectangle Container { get; }
            public abstract bool Contains(Point point);
            public virtual bool Contains(int x, int y) => Contains(new Point(x, y));
            public virtual void LoopThroughPoints(Action<Point> callback)
            {
                Rectangle cont = Container;
                for (int i = cont.Left; i <= cont.Right; i++)
                {
                    for (int j = cont.Top; j <= cont.Bottom; j++)
                    {
                        if (Contains(i, j))
                            callback.Invoke(new Point(i, j));
                    }
                }
            }
        }
        public class Triangle(Point a, Point b, Point c) : ITDShape
        {
            public Point A = a;
            public Point B = b;
            public Point C = c;
            public override Rectangle Container => GetBounding();
            public Rectangle GetBounding()
            {
                int minX = Math.Min(A.X, Math.Min(B.X, C.X));
                int minY = Math.Min(A.Y, Math.Min(B.Y, C.Y));
                int maxX = Math.Max(A.X, Math.Max(B.X, C.X));
                int maxY = Math.Max(A.Y, Math.Max(B.Y, C.Y));

                int width = maxX - minX;
                int height = maxY - minY;

                return new Rectangle(minX, minY, width, height);
            }
            public override bool Contains(Point point)
            {
                double denom = (B.Y - C.Y) * (A.X - C.X) + (C.X - B.X) * (A.Y - C.Y);
                double a = ((B.Y - C.Y) * (point.X - C.X) + (C.X - B.X) * (point.Y - C.Y)) / denom;
                double b = ((C.Y - A.Y) * (point.X - C.X) + (A.X - C.X) * (point.Y - C.Y)) / denom;
                double c = 1 - a - b;
                return a >= 0 && b >= 0 && c >= 0;
            }
        }
        public class Ellipse(int x, int y, int xRadius, int yRadius) : ITDShape
        {
            public int X = x;
            public int Y = y;
            public int XRadius = xRadius;
            public int YRadius = yRadius;
            public Point Center {  get { return new Point(X, Y); } }
            public override Rectangle Container => new(X - XRadius, Y - YRadius, XRadius * 2, YRadius * 2);
            public override bool Contains(Point point)
            {
                Point normalized = new(point.X - X, point.Y - Y);

                return ((double)(normalized.X * normalized.X) / (XRadius * XRadius)) + ((double)(normalized.Y * normalized.Y) / (YRadius * YRadius))
                    <= 1.0;
            }
        }
    }
}
