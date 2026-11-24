using FluentValidation;

namespace CriaLab.Application.Features.Identidade.Cadastrar;

public sealed class CadastrarValidador : AbstractValidator<CadastrarPedido>
{
    public CadastrarValidador()
    {
        RuleFor(x => x.NomeCompleto).NotEmpty().WithMessage("Informe o nome completo.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Informe o e-mail.").EmailAddress().WithMessage("Informe um e-mail válido.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Informe a senha.");
        RuleFor(x => x.ConfirmarSenha).Equal(x => x.Senha).WithMessage("As senhas não conferem.");
    }
}
