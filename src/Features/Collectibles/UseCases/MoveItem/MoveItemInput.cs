using Godot;
using MarioClone.Features.Collectibles.Models;

namespace MarioClone.Features.Collectibles.UseCases.MoveItem;

public record MoveItemInput(
    ItemKind Kind,
    Vector2 CurrentVelocity,
    int Direction,
    bool IsOnFloor,
    bool IsOnWall,
    float DeltaTime,
    float Gravity
);
