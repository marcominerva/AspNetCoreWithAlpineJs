using Microsoft.AspNetCore.Diagnostics;

namespace MinimalRazor.ExceptionHandlers;

public class DefaultExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        await problemDetailsService.WriteAsync(new()
        {
            HttpContext = httpContext,
            AdditionalMetadata = httpContext.Features.Get<IExceptionHandlerFeature>()?.Endpoint?.Metadata,
            ProblemDetails =
            {
                Status = httpContext.Response.StatusCode,
                Title = exception.GetType().FullName,
                Detail = exception.Message
            },
            Exception = exception
        });

        return true;
    }
}
