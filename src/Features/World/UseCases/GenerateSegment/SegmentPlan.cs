using System.Collections.Generic;
using Godot;

namespace MarioClone.Features.World.UseCases.GenerateSegment;

// Resultado del caso de uso: qué poner en el tramo y dónde termina
public record SegmentPlan(
    float EndX,
    Vector2 GroundPosition,
    List<Vector2> Platforms,
    List<Vector2> Coins,
    List<Vector2> Enemies
);
