using Microsoft.Xna.Framework;

namespace GMTK24.Model;

public readonly record struct Cell(int X, int Y)
{
    public static Cell Origin => new(0, 0);

    public Point ToPoint()
    {
        return new Point(X, Y);
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    public static Cell operator +(Cell a, Cell b)
    {
        return new Cell(a.X + b.X, a.Y + b.Y);
    }
    
    public static Cell operator -(Cell a, Cell b)
    {
        return new Cell(a.X - b.X, a.Y - b.Y);
    }
}
