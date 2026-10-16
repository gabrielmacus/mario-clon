using Godot;
using MarioClone.Features.Economy;
using MarioClone.Features.Enemies;
using MarioClone.Features.Player;
using MarioClone.Features.Player.UseCases.Die;
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

		// Flujo de la aplicación: el bootstrap decide qué pasa con la partida
		EventBus.Subscribe<GameOverEvent>(OnGameOver);

		GD.Print("[GameBootstrap] Todos los módulos inicializados con Casos de Uso Directos + EventBus.");
	}

	public override void _ExitTree()
	{
		EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
	}

	// Game over: congela el juego (Mario sigue animándose: su process_mode es Always),
	// deja ver la muerte y el cartel, y reinicia la escena
	private void OnGameOver(GameOverEvent e)
	{
		GetTree().Paused = true;
		GetTree().CreateTimer(4.0, processAlways: true).Timeout += Restart;
	}

	private void Restart()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
}
