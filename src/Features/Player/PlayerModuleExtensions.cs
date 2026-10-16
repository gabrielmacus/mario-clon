using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.AddLife;
using MarioClone.Features.Player.UseCases.TakeDamage;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player;

public static class PlayerModuleExtensions
{
    public static AddLifeHandler AddLife { get; private set; } = null!;
    public static TakeDamageHandler TakeDamage { get; private set; } = null!;

    public static void AddPlayerModule()
    {
        var repository = new PlayerRepository();
        AddLife = new AddLifeHandler(repository);
        TakeDamage = new TakeDamageHandler(repository);

        // Auto-wiring: C# infiere el evento BonusLifeAwardedEvent de la interfaz IEventHandler
        EventBus.Register(AddLife);
        EventBus.Register(TakeDamage);
    }
}
