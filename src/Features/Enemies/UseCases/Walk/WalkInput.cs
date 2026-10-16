using Godot;

namespace MarioClone.Features.Enemies.UseCases.Walk;

public record WalkInput(
    ulong EnemyId,
    Vector2 CurrentVelocity,
    int Direction,       // 1 derecha, -1 izquierda
    bool IsOnFloor,
    bool IsOnWall,
    float DeltaTime,
    float Gravity
);
