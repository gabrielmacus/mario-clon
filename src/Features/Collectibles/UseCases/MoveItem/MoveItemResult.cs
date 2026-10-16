using Godot;

namespace MarioClone.Features.Collectibles.UseCases.MoveItem;

// Direction: 1 derecha, -1 izquierda
public record MoveItemResult(Vector2 Velocity, int Direction);
