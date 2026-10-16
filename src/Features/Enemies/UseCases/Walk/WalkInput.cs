using Godot;

namespace MarioClone.Features.Enemies.UseCases.Walk;

public record WalkInput(
    ulong EnemyId,
    Vector2 CurrentVelocity,
    bool IsOnFloor,
    float DeltaTime,
    float Gravity
);
