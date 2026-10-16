using Godot;
using MarioClone.Features.Player.Models;

namespace MarioClone.Features.Player.UseCases.Locomotion;

// Lo que decide el caso de uso: cómo se mueve, en qué estado queda y hacia dónde mira (1 derecha, -1 izquierda)
public record LocomotionResult(Vector2 Velocity, MovementState State, int Facing);
