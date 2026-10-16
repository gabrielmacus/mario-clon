using Godot;
using MarioClone.Features.Economy;
using MarioClone.Features.Enemies;
using MarioClone.Features.Player;
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

		GD.Print("[GameBootstrap] Todos los módulos inicializados con Casos de Uso Directos + EventBus.");
	}
}
