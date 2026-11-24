using FluentValidation;

namespace CriaLab.Application.Features.Identidade.Entrar;

public sealed class EntrarValidador : AbstractValidator<EntrarPedido>
{
    public EntrarValidador()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Informe o e-mail.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Informe a senha.");
    }
}
