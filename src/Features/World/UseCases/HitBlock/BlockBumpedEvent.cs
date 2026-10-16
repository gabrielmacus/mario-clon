using Godot;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.World.UseCases.HitBlock;

// El bloque saltó pero no cambia (ladrillo común)
public record BlockBumpedEvent(Vector2I Cell) : IEvent;
