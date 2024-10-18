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
            public abstract bool Contains(Point point);
            public virtual bool Contains(int x, int y) => Contains(new Point(x, y));
            public abstract void LoopThroughPoints(Action<Point> callback);
            public abstract Rectangle Container { get; }
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
                Point normalized = new Point(point.X - X, point.Y - Y);

                return ((double)(normalized.X * normalized.X) / (XRadius * XRadius)) + ((double)(normalized.Y * normalized.Y) / (YRadius * YRadius))
                    <= 1.0;
            }
            public override void LoopThroughPoints(Action<Point> callback)
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
    }
}
