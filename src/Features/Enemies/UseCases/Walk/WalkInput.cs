using Godot;

namespace MarioClone.Features.Enemies.UseCases.Walk;

public record WalkInput(
    Vector2 CurrentVelocity,
    bool IsOnFloor,
    float DeltaTime,
    float Gravity
);
