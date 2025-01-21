using Humanizer;
using ITD.Content.Tiles.Unused;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
namespace ITD.Utilities
{
    public static class WorldSizeID
    {
        public const int Small = 0;
        public const int Medium = 1;
        public const int Large = 2;
    }
    public static class WorldGenHelpers
    {
        public static bool FirstY(int x, Func<Point, bool> match, out int y)
        {
            for (int i = 0; i < Main.maxTilesY; i++)
            {
                if (match(new Point(x, i)))
                {
                    y = i;
                    return true;
                }
            }
            y = 0;
            return false;
        }
        public static void LoopThroughPoints(this Rectangle rect, Action<Point> callback)
        {
            for (int i = rect.Left; i <= rect.Right; i++)
            {
                for (int j = rect.Top; j <= rect.Bottom; j++)
                {
                    callback.Invoke(new Point(i, j));
                }
            }
        }
        public static IEnumerable<Point> Points(this Rectangle rect)
        {
            for (int i = rect.Left; i <= rect.Right; i++)
            {
                for (int j = rect.Top; j <= rect.Bottom; j++)
                {
                    yield return new Point(i, j);
                }
            }
        }
        public static Point RandomPoint(this Rectangle rect, UnifiedRandom rand)
        {
            return new(rect.X + rand.Next(rect.Width + 1), rect.Y + rand.Next(rect.Height + 1));
        }
        public static void QuickDebugRectangle(Rectangle rect)
        {
            ushort debugTile = (ushort)ModContent.TileType<Debugger>();
            for (int i = rect.Left; i <= rect.Right; i++)
            {
                Tile t = Framing.GetTileSafely(i, rect.Top);
                t.HasTile = true;
                t.TileType = debugTile;
                Tile t2 = Framing.GetTileSafely(i, rect.Bottom);
                t2.HasTile = true;
                t2.TileType = debugTile;
            }
            for (int i = rect.Top; i <= rect.Bottom; i++)
            {
                Tile t = Framing.GetTileSafely(rect.Left, i);
                t.HasTile = true;
                t.TileType = debugTile;
                Tile t2 = Framing.GetTileSafely(rect.Right, i);
                t2.HasTile = true;
                t2.TileType = debugTile;
            }
        }
        public static class Procedural
        {
            public static Rectangle DigQuadTunnel(Point origin, Point end, int width, int quadAmount = 3, int randomRange = 0, Action<Point> callback = null, bool rounded = true)
            {
                // default callback: dig tunnel
                callback ??= p =>
                {
                    Tile t = Framing.GetTileSafely(p);
                    if (t.TileType != ModContent.TileType<Debugger>())
                        Framing.GetTileSafely(p).HasTile = false;
                };

                Vector2 originF = origin.ToVector2();
                Vector2 endF = end.ToVector2();
                Vector2 direction = endF - originF;

                Vector2 normalizedDirection = direction.SafeNormalize(Vector2.Zero);
                float segmentLength = direction.Length() / quadAmount;
                Vector2 sizeDir = normalizedDirection * segmentLength;

                QuadDigData prev = new();
                Vector2 currentPosition = originF;
                int currentWidth1 = width;

                for (int i = 0; i < quadAmount; i++)
                {
                    if (i == 0)
                    {
                        prev = DigDirectionQuad(currentPosition.ToPoint(), sizeDir, width, width, randomRange, true, callback);
                        currentWidth1 = prev.Width2;
                        if (rounded)
                            new ITDShapes.Ellipse(origin.X, origin.Y, prev.Width1, prev.Width1).LoopThroughPoints(callback);
                    }
                    else
                    {
                        prev = DigDirectionQuad(prev.End, sizeDir, currentWidth1, width, randomRange, false, callback);
                        currentWidth1 = prev.Width2;
                    }
                    currentPosition += sizeDir;
                }
                if (rounded)
                    new ITDShapes.Ellipse(prev.End.X, prev.End.Y, prev.Width2, prev.Width2).LoopThroughPoints(callback);

                int dir = Math.Sign(direction.X);
                // note: this only works for straight tunnels (like those in the DD)
                return new Rectangle(origin.X + Math.Min(0, (int)direction.X), origin.Y - width / 2, (int)direction.Length(), width * 2);
            }
            public static QuadDigData DigDirectionQuad(Point origin, Vector2 sizeDirection, int width, int width2 = -1, int randomRange = 0, bool randomizeWidth1 = false, Action<Point> callback = null)
            {
                // default callback: dig tunnel
                callback ??= p =>
                {
                    Tile t = Framing.GetTileSafely(p);
                    if (t.TileType != ModContent.TileType<Debugger>())
                        Framing.GetTileSafely(p).HasTile = false;
                };

                if (randomizeWidth1)
                    width += WorldGen.genRand.Next(-randomRange, randomRange + 1);
                if (width2 == -1)
                    width2 = width;
                width2 += WorldGen.genRand.Next(-randomRange, randomRange + 1);

                Vector2 normalizedDirection = sizeDirection.SafeNormalize(Vector2.Zero);
                float quadLength = sizeDirection.Length();

                Point endPoint = new Vector2(origin.X + normalizedDirection.X * quadLength, origin.Y + normalizedDirection.Y * quadLength).ToPoint();

                Vector2 perpendicular = normalizedDirection.RotatedBy(-MathHelper.PiOver2);

                // clockwise listed
                // width1
                Point corner4 = new Vector2(origin.X + (int)(perpendicular.X * width), origin.Y + (int)(perpendicular.Y * width)).ToPoint();
                Point corner1 = new Vector2(origin.X - (int)(perpendicular.X * width), origin.Y - (int)(perpendicular.Y * width)).ToPoint();

                // width2
                Point corner3 = new Vector2(endPoint.X + (int)(perpendicular.X * width2), endPoint.Y + (int)(perpendicular.Y * width2)).ToPoint();
                Point corner2 = new Vector2(endPoint.X - (int)(perpendicular.X * width2), endPoint.Y - (int)(perpendicular.Y * width2)).ToPoint();

                new ITDShapes.Quad(corner1, corner2, corner3, corner4).LoopThroughPoints(callback);

                return new QuadDigData(width, width2, origin, endPoint);
            }
            public struct QuadDigData(int width1, int width2, Point origin, Point end)
            {
                public int Width1 = width1;
                public int Width2 = width2;
                public Point Origin = origin;
                public Point End = end;
            }
        }
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
                cont.LoopThroughPoints(p =>
                {
                    if (Contains(p))
                        callback(p);
                });
            }
            public virtual IEnumerable<Point> Points()
            {
                return Container.Points().Where(Contains);
            }
            public virtual Point RandomPoint(UnifiedRandom rand)
            {
                return Container.RandomPoint(rand);
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
        public class Quad(Point a, Point b, Point c, Point d) : ITDShape
        {
            public Point A = a;
            public Point B = b;
            public Point C = c;
            public Point D = d;
            public Triangle TriangleA { get { return new Triangle(A, B, D); } }
            public Triangle TriangleB { get { return new Triangle(B, C, D); } }
            public override Rectangle Container => MiscHelpers.ContainsRectangles(TriangleA.Container, TriangleB.Container);
            public override bool Contains(Point point)
            {
                return TriangleA.Contains(point) || TriangleB.Contains(point);
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
            public override Point RandomPoint(UnifiedRandom rand)
            {
                double theta = rand.NextDouble() * Math.Tau;
                double radRand = Math.Sqrt(rand.NextDouble());
                return new((int)(X + radRand * XRadius * Math.Cos(theta)), (int)(Y + radRand * YRadius * Math.Sin(theta)));
            }
            public float GetDistanceToEdge(Point point)
            {
                double normalizedX = (double)(point.X - X) / XRadius;
                double normalizedY = (double)(point.Y - Y) / YRadius;

                double magnitude = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);

                magnitude = Math.Max(0, magnitude);

                double edgeX = normalizedX / magnitude * XRadius;
                double edgeY = normalizedY / magnitude * YRadius;

                double deltaX = point.X - (X + edgeX);
                double deltaY = point.Y - (Y + edgeY);

                return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
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
                return MiscHelpers.ContainsRectangles(mainParabola.Container, secondParabola.Container);
            }
            public override bool Contains(Point point)
            {
                return mainParabola.Contains(point) && !secondParabola.Contains(point);
            }
        }
    }
}
