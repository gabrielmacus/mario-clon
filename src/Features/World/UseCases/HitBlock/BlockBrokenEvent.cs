using Godot;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.World.UseCases.HitBlock;

// Un ladrillo común se rompió (lo golpeó el jugador grande)
public record BlockBrokenEvent(Vector2I Cell) : IEvent;
