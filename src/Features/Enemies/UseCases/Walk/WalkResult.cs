using Godot;
using MarioClone.Features.Enemies.Models;

namespace MarioClone.Features.Enemies.UseCases.Walk;

// Lo que decide el caso de uso: cómo se mueve, hacia dónde va y en qué estado está (para la View)
public record WalkResult(Vector2 Velocity, int Direction, EnemyStatus Status);
