using Microsoft.AspNetCore.Mvc.Filters;

namespace LoggingExample.Web.Filters
{
	/// <summary>
	/// FluentValidation istisnaları için özel exception filter
	/// </summary>
	public class FluentValidationExceptionFilter : IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			if (context.Exception is FluentValidation.ValidationException validationException)
			{
				// FluentValidation hatalarını kendi ValidationException sınıfımıza dönüştür
				var errors = validationException.Errors
					.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
					.ToList();

				context.Exception = new LoggingExample.Web.Models.Exceptions.ValidationException(
					"Doğrulama hatası oluştu", errors);
			}
		}
	}
}