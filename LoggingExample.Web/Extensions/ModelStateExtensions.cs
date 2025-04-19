using Microsoft.AspNetCore.Mvc.ModelBinding;
using LoggingExample.Web.Models.Exceptions;

namespace LoggingExample.Web.Extensions
{
    /// <summary>
    /// ModelState validation için extension metodları
    /// </summary>
    public static class ModelStateExtensions
    {
        /// <summary>
        /// ModelState'deki hataları ValidationException'a dönüştürür
        /// </summary>
        public static void ThrowIfInvalid(this ModelStateDictionary modelState)
        {
            if (modelState.IsValid)
                return;

            var errors = modelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(error => 
                    !string.IsNullOrWhiteSpace(error.ErrorMessage) 
                        ? $"{x.Key}: {error.ErrorMessage}" 
                        : error.Exception?.Message ?? "Bilinmeyen hata"))
                .ToList();

            throw new ValidationException("Model doğrulama hatası", errors);
        }
    }
} 