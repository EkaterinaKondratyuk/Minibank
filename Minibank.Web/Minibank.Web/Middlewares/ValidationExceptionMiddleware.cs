using Microsoft.AspNetCore.Http;
using Minibank.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Minibank.Web.Middlewares
{
    public class ValidationExceptionMiddleware
    {
        public readonly RequestDelegate Next;

        public ValidationExceptionMiddleware(RequestDelegate next)
        {
            this.Next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await Next(httpContext);
            }
            catch (ValidationException exception)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new { exception.Message });
            }
            catch (FluentValidation.ValidationException exception)
            {
                var errors = exception.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}");
                var errorMessage = string.Join(Environment.NewLine, errors);

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new { Message = errorMessage });
            }
            catch (ObjectNotFoundException exception)
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsJsonAsync(new { exception.Message });
            }
            catch (BadHttpRequestException exception)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new { exception.Message });
            }
            catch
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new { Message = "Внутренняя ошибка сервера" });
            }
        }
    }
}
