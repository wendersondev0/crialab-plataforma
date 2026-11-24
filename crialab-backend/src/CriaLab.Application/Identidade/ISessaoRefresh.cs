using CriaLab.Domain.Identidade;

namespace CriaLab.Application.Identidade;

public interface ISessaoRefresh
{
    Task GuardarAsync(UsuarioAplicacao usuario, string refreshToken);
    Task<UsuarioAplicacao?> BuscarPorRefreshAsync(string refreshToken);
    Task RemoverAsync(UsuarioAplicacao usuario);
}
