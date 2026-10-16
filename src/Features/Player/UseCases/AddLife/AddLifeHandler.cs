using MarioClone.Features.Economy.UseCases.CollectCoin;
using MarioClone.Features.Player.Events;
using MarioClone.Features.Player.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.AddLife;

public class AddLifeHandler(PlayerRepository playerRepository) : IEventHandler<BonusLifeAwardedEvent>
{
	// Único método: reacciona al evento desacoplado
	public void Handle(BonusLifeAwardedEvent @event)
	{
		var player = playerRepository.GetPlayer(@event.PlayerId);
		player.Lives += 1;

		EventBus.Publish(new PlayerLivesChangedEvent(@event.PlayerId, player.Lives));
	}
}
