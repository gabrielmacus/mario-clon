using System.Collections.Generic;
using Godot;

namespace MarioClone.Features.World.UseCases.LoadLevel;

// Resultado del caso de uso: qué hay en cada celda del nivel (en celdas de 16x16)
public record LevelPlan(
    int Width,
    int Height,
    Vector2I PlayerStart,
    List<TilePlacement> Tiles,
    List<EntityPlacement> Entities
);
