using MarioClone.Features.Collectibles.UseCases.CollectItem;
using MarioClone.Features.Economy.UseCases.CollectCoin;
using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.AddLife;
using MarioClone.Features.Player.UseCases.Bounce;
using MarioClone.Features.Player.UseCases.Die;
using MarioClone.Features.Player.UseCases.FallIntoPit;
using MarioClone.Features.Player.UseCases.FinishLevel;
using MarioClone.Features.Player.UseCases.GetLives;
using MarioClone.Features.Player.UseCases.Locomotion;
using MarioClone.Features.Player.UseCases.PowerUp;
using MarioClone.Features.Player.UseCases.TakeDamage;
using MarioClone.Features.Player.UseCases.UpdateInvulnerability;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player;

public static class PlayerModuleExtensions
{
    // El repositorio dura toda la partida: sobrevive a la recarga de la escena al perder una vida
    private static PlayerRepository? _repository;

    public static AddLifeHandler AddLife { get; private set; } = null!;
    public static TakeDamageHandler TakeDamage { get; private set; } = null!;
    public static BounceHandler Bounce { get; private set; } = null!;
    public static UpdateLocomotionHandler Locomotion { get; private set; } = null!;
    public static DieHandler Die { get; private set; } = null!;
    public static FallIntoPitHandler FallIntoPit { get; private set; } = null!;
    public static FinishLevelHandler FinishLevel { get; private set; } = null!;
    public static PowerUpHandler PowerUp { get; private set; } = null!;
    public static UpdateInvulnerabilityHandler UpdateInvulnerability { get; private set; } = null!;
    public static GetLivesHandler GetLives { get; private set; } = null!;

    public static void AddPlayerModule()
    {
        _repository ??= new PlayerRepository();
        _repository.StartNewLife();
        var repository = _repository;

        AddLife = new AddLifeHandler(repository);
        TakeDamage = new TakeDamageHandler(repository);
        Bounce = new BounceHandler(repository);
        Die = new DieHandler(repository);
        FallIntoPit = new FallIntoPitHandler();
        FinishLevel = new FinishLevelHandler(repository);
        PowerUp = new PowerUpHandler(repository);
        Locomotion = new UpdateLocomotionHandler(repository);
        UpdateInvulnerability = new UpdateInvulnerabilityHandler(repository);
        GetLives = new GetLivesHandler(repository);

        // Auto-wiring: C# infiere el evento de la interfaz IEventHandler<T>
        EventBus.Register<BonusLifeAwardedEvent>(AddLife);   // escucha dos eventos: se indica cuál en cada registro
        EventBus.Register<ItemCollectedEvent>(AddLife);
        EventBus.Register(TakeDamage);
        EventBus.Register(Bounce);
        EventBus.Register(Die);
        EventBus.Register(FinishLevel);
        EventBus.Register(PowerUp);
    }

    // Partida nueva (game over o nivel completado): se descartan vidas y poder
    public static void ResetSession() => _repository = null;
}
