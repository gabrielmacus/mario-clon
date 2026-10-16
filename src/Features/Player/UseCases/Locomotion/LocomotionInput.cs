using Godot;

namespace MarioClone.Features.Player.UseCases.Locomotion;

// DTO inmutable con todos los estímulos que necesita el caso de uso para calcular la física
public record LocomotionInput(
    Vector2 CurrentVelocity,
    float Direction,
    bool JumpRequested,
    bool IsOnFloor,
    float DeltaTime,
    float Gravity
);
