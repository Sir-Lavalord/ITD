using System;
using System.Diagnostics.CodeAnalysis;
using Terraria.DataStructures;

namespace ITD.Systems.DataStructures;

public struct Point8
{
    public readonly sbyte X;
    public readonly sbyte Y;
    public static readonly Point8 Zero;
    public static readonly Point8 One = new(1, 1);
    public Point8(int v)
    {
        X = (sbyte)v;
        Y = (sbyte)v;
    }
    public Point8(short v)
    {
        X = (sbyte)v;
        Y = (sbyte)v;
    }
    public Point8(sbyte v)
    {
        X = v;
        Y = v;
    }
    public Point8(int x, int y)
    {
        X = (sbyte)x;
        Y = (sbyte)y;
    }
    public Point8(short x, short y)
    {
        X = (sbyte)x;
        Y = (sbyte)y;
    }
    public Point8(sbyte x, sbyte y)
    {
        X = x;
        Y = y;
    }
    public Point8(Point p)
    {
        X = (sbyte)p.X;
        Y = (sbyte)p.Y;
    }
    public Point8(Point16 p)
    {
        X = (sbyte)p.X;
        Y = (sbyte)p.Y;
    }
    public static bool operator ==(Point8 first, Point8 second)
    {
        return first.X == second.X && first.Y == second.Y;
    }
    public static bool operator !=(Point8 first, Point8 second)
    {
        return !(first == second);
    }
    public static bool operator >(Point8 first, Point8 second)
    {
        return (first.X > second.X) && (first.Y > second.Y);
    }
    public static bool operator <(Point8 first, Point8 second)
    {
        return !(first > second);
    }
    public static Point8 operator +(Point8 first, Point8 second)
    {
        return new(first.X + second.X, first.Y + second.Y);
    }
    public static Point8 operator -(Point8 first, Point8 second)
    {
        return new(first.X - second.X, first.Y - second.Y);
    }
    public override readonly bool Equals([NotNullWhen(true)] object obj)
    {
        return obj is Point8 other && other == this;
    }
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
    public override readonly string ToString() => $"{{{X}, {Y}}}";
}
