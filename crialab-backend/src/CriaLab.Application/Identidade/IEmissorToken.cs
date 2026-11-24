using CriaLab.Domain.Identidade;

namespace CriaLab.Application.Identidade;

public interface IEmissorToken
{
    string CriarAccessToken(UsuarioAplicacao usuario, IEnumerable<string> papeis);
    string CriarRefreshToken();
}
