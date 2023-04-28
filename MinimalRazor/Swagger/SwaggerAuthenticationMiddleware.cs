using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using MinimaRazor.BusinessLayer.Settings;
using TinyHelpers.Extensions;

namespace MinimalRazor.Swagger;

public class SwaggerAuthenticationMiddleware
{
    private readonly RequestDelegate next;
    private readonly SwaggerSettings settings;

    public SwaggerAuthenticationMiddleware(RequestDelegate next, IOptions<SwaggerSettings> swaggerSettingsOptions)
    {
        this.next = next;
        settings = swaggerSettingsOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") && settings.UserName.HasValue() && settings.Password.HasValue())
        {
            string authenticationHeader = context.Request.Headers[HeaderNames.Authorization];
            if (authenticationHeader?.StartsWith("Basic ") ?? false)
            {
                var header = AuthenticationHeaderValue.Parse(authenticationHeader);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter)).Split(':', count: 2);
                var userName = credentials.ElementAtOrDefault(0);
                var password = credentials.ElementAtOrDefault(1);

                if (userName == settings.UserName && password == settings.Password)
                {
                    await next.Invoke(context);
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = new StringValues("Basic");
        }
        else
        {
            await next.Invoke(context);
        }
    }
}
