using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.Locomotion;

// Evento de Dominio emitido cuando el caso de uso valida y ejecuta un salto exitoso
public record PlayerJumpedEvent(int PlayerId = 1) : IEvent;
