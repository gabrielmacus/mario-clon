using System.Collections.Generic;

namespace MarioClone.Features.Player.Models;

public class PlayerState
{
    private readonly Dictionary<int, PlayerData> _players = new();

    public PlayerData GetPlayer(int playerId = 1)
    {
        if (!_players.TryGetValue(playerId, out var player))
        {
            player = new PlayerData();
            _players[playerId] = player;
        }
        return player;
    }

    public void Reset()
    {
        _players.Clear();
    }
}
