using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.AddLife;
using MarioClone.Features.Player.UseCases.Bounce;
using MarioClone.Features.Player.UseCases.Die;
using MarioClone.Features.Player.UseCases.GetLives;
using MarioClone.Features.Player.UseCases.Locomotion;
using MarioClone.Features.Player.UseCases.TakeDamage;
using MarioClone.Features.Player.UseCases.UpdateInvulnerability;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player;

public static class PlayerModuleExtensions
{
    public static AddLifeHandler AddLife { get; private set; } = null!;
    public static TakeDamageHandler TakeDamage { get; private set; } = null!;
    public static BounceHandler Bounce { get; private set; } = null!;
    public static UpdateLocomotionHandler Locomotion { get; private set; } = null!;
    public static DieHandler Die { get; private set; } = null!;
    public static UpdateInvulnerabilityHandler UpdateInvulnerability { get; private set; } = null!;
    public static GetLivesHandler GetLives { get; private set; } = null!;

    public static void AddPlayerModule()
    {
        var repository = new PlayerRepository();
        AddLife = new AddLifeHandler(repository);
        TakeDamage = new TakeDamageHandler(repository);
        Bounce = new BounceHandler(repository);
        Die = new DieHandler(repository);
        Locomotion = new UpdateLocomotionHandler(repository);
        UpdateInvulnerability = new UpdateInvulnerabilityHandler(repository);
        GetLives = new GetLivesHandler(repository);

        // Auto-wiring: C# infiere el evento BonusLifeAwardedEvent de la interfaz IEventHandler
        EventBus.Register(AddLife);
        EventBus.Register(TakeDamage);
        EventBus.Register(Bounce);
        EventBus.Register(Die);
    }
}
