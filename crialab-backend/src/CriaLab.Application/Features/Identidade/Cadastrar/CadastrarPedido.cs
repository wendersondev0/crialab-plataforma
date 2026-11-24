namespace CriaLab.Application.Features.Identidade.Cadastrar;

public sealed record CadastrarPedido(
    string NomeCompleto,
    string Email,
    string? Telefone,
    string Senha,
    string ConfirmarSenha);
