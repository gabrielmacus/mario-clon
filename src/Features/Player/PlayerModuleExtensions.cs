using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.AddLife;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player;

public static class PlayerModuleExtensions
{
    public static AddLifeHandler AddLife { get; private set; } = null!;

    public static void AddPlayerModule()
    {
        var state = new PlayerState();
        AddLife = new AddLifeHandler(state);

        // Auto-wiring: C# infiere el evento BonusLifeAwardedEvent de la interfaz IEventHandler
        EventBus.Register(AddLife);
    }
}
