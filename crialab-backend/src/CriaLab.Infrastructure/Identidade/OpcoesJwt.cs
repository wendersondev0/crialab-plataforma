namespace CriaLab.Infrastructure.Identidade;

public sealed class OpcoesJwt
{
    public const string Secao = "Jwt";
    public string Emissor { get; set; } = "crialab-api";
    public string Publico { get; set; } = "crialab-pwa";
    public string Chave { get; set; } = string.Empty;
    public int AccessTokenMinutos { get; set; } = 15;
    public int RefreshTokenDias { get; set; } = 14;
    public string CookieRefresh { get; set; } = "crialab_refresh";
}
