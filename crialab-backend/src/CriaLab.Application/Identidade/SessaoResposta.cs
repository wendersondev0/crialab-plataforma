namespace CriaLab.Application.Identidade;

public sealed record SessaoResposta(string AccessToken, UsuarioResposta Usuario, string RefreshToken);
