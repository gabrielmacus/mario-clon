using Godot;
using MarioClone.Features.Collectibles;
using MarioClone.Features.Economy;
using MarioClone.Features.Enemies;
using MarioClone.Features.Player;
using MarioClone.Features.Player.UseCases.Die;
using MarioClone.Features.World;
using MarioClone.Features.World.UseCases.CompleteLevel;
using MarioClone.Shared.Messaging;

namespace MarioClone.Shared.Bootstrap;

public partial class GameBootstrap : Node
{
	public override void _EnterTree()
	{
		// Limpieza de suscriptores al recargar la escena
		EventBus.Clear();

		// Inicialización modular de dependencias
		EconomyModuleExtensions.AddEconomyModule();
		PlayerModuleExtensions.AddPlayerModule();
		EnemiesModuleExtensions.AddEnemiesModule();
		WorldModuleExtensions.AddWorldModule();
		CollectiblesModuleExtensions.AddCollectiblesModule();

		// Flujo de la aplicación: el bootstrap decide qué pasa con la partida
		EventBus.Subscribe<LevelRestartEvent>(OnLevelRestart);
		EventBus.Subscribe<GameOverEvent>(OnGameOver);
		EventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);

		GD.Print("[GameBootstrap] Todos los módulos inicializados con Casos de Uso Directos + EventBus.");
	}

	public override void _ExitTree()
	{
		EventBus.Unsubscribe<LevelRestartEvent>(OnLevelRestart);
		EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
		EventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
	}

	// Perdió una vida y le quedan otras: congela el juego, deja ver la muerte y reintenta el nivel
	private void OnLevelRestart(LevelRestartEvent e)
	{
		GetTree().Paused = true;
		ReloadAfter(3.0, newGame: false);
	}

	// Sin vidas: congela el juego, deja ver la muerte y el cartel, y arranca una partida nueva
	private void OnGameOver(GameOverEvent e)
	{
		GetTree().Paused = true;
		ReloadAfter(4.0, newGame: true);
	}

	// Llegó a la bandera: el juego sigue (Mario ya no tiene controles), muestra el cartel y arranca de nuevo
	private void OnLevelCompleted(LevelCompletedEvent e)
	{
		ReloadAfter(4.0, newGame: true);
	}

	private void ReloadAfter(double seconds, bool newGame)
	{
		GetTree().CreateTimer(seconds, processAlways: true).Timeout += () =>
		{
			// Partida nueva: se descartan vidas, poder, monedas y puntaje
			if (newGame)
			{
				PlayerModuleExtensions.ResetSession();
				EconomyModuleExtensions.ResetSession();
			}

			GetTree().Paused = false;
			GetTree().ReloadCurrentScene();
		};
	}
}
