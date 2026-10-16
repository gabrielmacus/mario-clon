using MarioClone.Features.Collectibles.Models;
using MarioClone.Features.Collectibles.UseCases.CollectItem;
using MarioClone.Features.Economy.UseCases.CollectCoin;
using MarioClone.Features.Player.Events;
using MarioClone.Features.Player.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.AddLife;

public class AddLifeHandler(PlayerRepository playerRepository)
	: IEventHandler<BonusLifeAwardedEvent>, IEventHandler<ItemCollectedEvent>
{
	// 100 monedas
	public void Handle(BonusLifeAwardedEvent @event) => GrantLife(@event.PlayerId);

	// Hongo verde (1UP)
	public void Handle(ItemCollectedEvent @event)
	{
		if (@event.Kind == ItemKind.OneUp) GrantLife(@event.PlayerId);
	}

	private void GrantLife(int playerId)
	{
		var player = playerRepository.GetPlayer(playerId);
		player.Lives += 1;

		EventBus.Publish(new PlayerLivesChangedEvent(playerId, player.Lives));
	}
}
