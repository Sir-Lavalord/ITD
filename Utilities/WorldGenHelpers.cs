using Humanizer;
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
            public bool Contains(int x, int y) => Contains(new Point(x, y));
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
            public Point Center => new(X, Y);
            public override Rectangle Container => new(X - XRadius, Y - YRadius, XRadius * 2, YRadius * 2);
            public override bool Contains(Point point)
            {
                Point normalized = new(point.X - X, point.Y - Y);

                return ((double)(normalized.X * normalized.X) / (XRadius * XRadius)) + ((double)(normalized.Y * normalized.Y) / (YRadius * YRadius))
                    <= 1.0;
            }
        }
        public class Parabola : ITDShape
        {
            public int X;
            public int Y;
            public int focusX;
            public int focusY;
            public int limitSize;
            public Point Vertex => new(X, Y);
            public Point Focus => new(focusX, focusY);
            public double A { get; private set; }
            public double B { get; private set; }
            public double C { get; private set; }
            public Parabola(int x, int y, int focusX, int focusY, int sizeLimit)
            {
                X = x;
                Y = y;
                this.focusX = focusX;
                this.focusY = focusY;
                limitSize = sizeLimit;
                CalculateCoefficients();
            }
            private void CalculateCoefficients()
            {
                A = 1.0 / (2 * (focusY - Y));
                B = -2 * A * X;
                C = A * Math.Pow(X, 2) + Y;
            }
            public override Rectangle Container => GetBounding();
            public Rectangle GetBounding()
            {
                int minX = X - limitSize;
                int maxX = X + limitSize;

                int minY;
                int maxY;

                if (focusY < Y) // upward parabola
                {
                    minY = Y - limitSize;
                    maxY = Y;
                }
                else // downward
                {
                    minY = Y;
                    maxY = Y + limitSize;
                }

                int width = maxX - minX;
                int height = maxY - minY;

                return new Rectangle(minX, minY, width, height);
            }

            public override bool Contains(Point point)
            {
                double yParabola = A * Math.Pow(point.X, 2) + B * point.X + C;
                bool b = focusY > Y ? point.Y > yParabola : point.Y < yParabola;
                return b;
            }
        }
        public class Banana : ITDShape
        {
            public Parabola mainParabola;
            public int secondParabolaY;
            public double innerFocusFactor;
            private Parabola secondParabola;
            public Banana(Parabola main, int secondY, double innerFocus)
            {
                mainParabola = main;
                secondParabolaY = secondY;
                innerFocusFactor = innerFocus;

                bool isUpward = mainParabola.focusY > mainParabola.Y;
                int mainFocusDistance = Math.Abs(mainParabola.focusY - mainParabola.Y);
                int secondFocusY = secondParabolaY + (isUpward ? 1 : -1) * (int)(mainFocusDistance * innerFocusFactor);

                secondParabola = new(mainParabola.X, secondY, mainParabola.focusX, secondFocusY, mainParabola.limitSize);
            }
            public override Rectangle Container => GetBounding();
            public Rectangle GetBounding()
            {
                Rectangle rect1 = mainParabola.Container;
                Rectangle rect2 = secondParabola.Container;

                int minX = Math.Min(rect1.X, rect2.X);
                int minY = Math.Min(rect1.Y, rect2.Y);

                int maxRight = Math.Max(rect1.Right, rect2.Right);
                int maxBottom = Math.Max(rect1.Bottom, rect2.Bottom);

                int width = maxRight - minX;
                int height = maxBottom - minY;

                return new Rectangle(minX, minY, width, height);
            }
            public override bool Contains(Point point)
            {
                return mainParabola.Contains(point) && !secondParabola.Contains(point);
            }
        }
    }
}
