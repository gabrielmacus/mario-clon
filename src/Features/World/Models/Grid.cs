using Godot;

namespace MarioClone.Features.World.Models;

// Medidas de la grilla del nivel
public static class Grid
{
    public const int TileSize = 16;

    public static Vector2I ToCell(Vector2 position) =>
        new(Mathf.FloorToInt(position.X / TileSize), Mathf.FloorToInt(position.Y / TileSize));
}
