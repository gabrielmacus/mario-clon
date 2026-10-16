using Godot;
using MarioClone.Features.World.Models;

namespace MarioClone.Features.World.UseCases.LoadLevel;

public record EntityPlacement(Vector2I Cell, EntityKind Kind);
