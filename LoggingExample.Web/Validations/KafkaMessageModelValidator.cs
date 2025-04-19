using FluentValidation;
using LoggingExample.Web.Models.Kafka;

namespace LoggingExample.Web.Validations
{
    /// <summary>
    /// KafkaMessageModel modeli için FluentValidation validator
    /// </summary>
    public class KafkaMessageModelValidator : AbstractValidator<KafkaMessageModel>
    {
        public KafkaMessageModelValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id alanı zorunludur");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Mesaj içeriği zorunludur")
                .MaximumLength(1000).WithMessage("Mesaj 1000 karakterden uzun olamaz");

            RuleFor(x => x.CreatedAt)
                .NotNull().WithMessage("Oluşturulma tarihi zorunludur")
                .Must(BeAValidDate).WithMessage("Geçerli bir tarih olmalıdır");
        }

        private bool BeAValidDate(DateTime date)
        {
            return date != default && date <= DateTime.UtcNow.AddDays(1);
        }
    }
} 