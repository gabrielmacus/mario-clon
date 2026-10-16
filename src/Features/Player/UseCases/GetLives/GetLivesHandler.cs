using MarioClone.Features.Player.Models;

namespace MarioClone.Features.Player.UseCases.GetLives;

// Consulta: solo lee, no modifica nada ni publica eventos
public class GetLivesHandler(PlayerRepository playerRepository)
{
    public int Handle(int playerId = 1) => playerRepository.GetPlayer(playerId).Lives;
}
