using FluentValidation;
using LoggingExample.Web.Models;

namespace LoggingExample.Web.Validations
{
    /// <summary>
    /// UserDto modeli için FluentValidation validator
    /// </summary>
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Kullanıcı adı zorunludur")
                .Length(3, 50).WithMessage("Kullanıcı adı 3-50 karakter arasında olmalıdır");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi zorunludur")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre zorunludur")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir");

            When(x => x.Age.HasValue, () =>
            {
                RuleFor(x => x.Age.Value)
                    .InclusiveBetween(18, 120).WithMessage("Yaş 18-120 arasında olmalıdır");
            });
        }
    }
} 