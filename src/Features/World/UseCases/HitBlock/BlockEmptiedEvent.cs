using Godot;
using MarioClone.Features.World.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.World.UseCases.HitBlock;

// El bloque saltó, soltó su contenido y quedó usado
public record BlockEmptiedEvent(Vector2I Cell, BlockContent Content, int PlayerId) : IEvent;
