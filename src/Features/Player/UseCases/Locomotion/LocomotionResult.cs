using Godot;
using MarioClone.Features.Player.Models;

namespace MarioClone.Features.Player.UseCases.Locomotion;

// Lo que decide el caso de uso: cómo se mueve, en qué estado queda, hacia dónde mira (1 derecha, -1 izquierda),
// si es grande (tamaño y colisión) y si tiene estrella
public record LocomotionResult(Vector2 Velocity, MovementState State, int Facing, bool IsBig, bool IsInvincible);
