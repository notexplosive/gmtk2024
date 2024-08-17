using System;
using ExplogineMonoGame.Data;
using GMTK24.Model;
using Microsoft.Xna.Framework;

namespace GMTK24;

public static class Grid
{
    public static int CellSize => 16;

    public static Vector2 SnapToGrid(Vector2 pixelPosition)
    {
        return CellToPixel(PixelToCell(pixelPosition));
    }

    public static Cell PixelToCell(Vector2 worldPosition)
    {
        int FloorToInt(float value)
        {
            return (int) Math.Floor(value);
        }

        return new Cell(FloorToInt(worldPosition.X / CellSize),
            FloorToInt(worldPosition.Y / CellSize));
    }

    public static Vector2 CellToPixel(Cell gridPosition)
    {
        return gridPosition.ToVector2() * CellSize;
    }

    public static RectangleF CellToPixelRectangle(Cell cell)
    {
        return new RectangleF(Grid.CellToPixel(cell), new Vector2(Grid.CellSize));
    }
}
