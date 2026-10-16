using Godot;
using MarioClone.Features.World.Models;

namespace MarioClone.Features.World.UseCases.LoadLevel;

public record TilePlacement(Vector2I Cell, TileKind Kind, BlockContent Content);
