using Microsoft.AspNetCore.Identity;

namespace CriaLab.Infrastructure.Identidade;

public sealed class DescritorErrosIdentidade : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = "Este e-mail já está em uso." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = "Este e-mail já está em uso." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"A senha deve ter no mínimo {length} caracteres." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "A senha deve conter ao menos um dígito." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "A senha deve conter ao menos uma letra minúscula." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "A senha deve conter ao menos uma letra maiúscula." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "A senha deve conter ao menos um caractere não alfanumérico." };
}
