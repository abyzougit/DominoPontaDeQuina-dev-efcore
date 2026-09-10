namespace DominoPontaDeQuina.WebApi.Models;

public sealed record RegistrarJogadorRequest(string Nome, Guid? UsuarioId = null);
