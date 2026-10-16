namespace MarioClone.Features.World.Models;

// Bloques sólidos del nivel. El orden coincide con las columnas de assets/tiles.png
public enum TileKind
{
    Ground,
    Brick,
    Question,
    Stair,
    PipeTopLeft,
    PipeTopRight,
    PipeBodyLeft,
    PipeBodyRight,
    Used           // bloque ya golpeado (vacío)
}
