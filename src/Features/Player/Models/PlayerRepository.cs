using System.Collections.Generic;

namespace MarioClone.Features.Player.Models;

public class PlayerRepository
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

    // Nuevo intento del nivel: se conservan vidas y poder, se reinicia lo de la vida anterior
    public void StartNewLife()
    {
        foreach (var player in _players.Values) player.StartNewLife();
    }
}
