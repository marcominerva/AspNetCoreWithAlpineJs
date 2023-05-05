using Microsoft.AspNetCore.Diagnostics;
using MinimalHelpers.OpenApi;
using MinimalRazor.Endpoints;
using MinimalRazor.Extensions;
using MinimalRazor.Swagger;
using MinimaRazor.BusinessLayer.Settings;
using TinyHelpers.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var swagger = builder.Services.ConfigureAndGet<SwaggerSettings>(builder.Configuration, nameof(SwaggerSettings));

builder.Services.AddRazorPages();

if (swagger.IsEnabled)
{
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.AddMissingSchemas();
    });
}

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseWhen(context => context.IsWebRequest(), builder =>
{
    if (!app.Environment.IsDevelopment())
    {
        builder.UseExceptionHandler("/Errors/500");

        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        builder.UseHsts();
    }

    builder.UseStatusCodePagesWithReExecute("/errors/{0}");
});

app.UseWhen(context => context.IsApiRequest(), builder =>
{
    if (!app.Environment.IsDevelopment())
    {
        // Error handling
        builder.UseExceptionHandler(new ExceptionHandlerOptions
        {
            AllowStatusCode404Response = true,
            ExceptionHandler = async (HttpContext context) =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var error = exceptionHandlerFeature?.Error;

                if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
                {
                    // Write as JSON problem details
                    await problemDetailsService.WriteAsync(new()
                    {
                        HttpContext = context,
                        AdditionalMetadata = exceptionHandlerFeature?.Endpoint?.Metadata,
                        ProblemDetails =
                        {
                            Status = context.Response.StatusCode,
                            Title = error?.GetType().FullName ?? "An error occurred while processing your request",
                            Detail = error?.Message
                        }
                    });
                }
            }
        });
    }

    builder.UseStatusCodePages();
});

app.UseStaticFiles();

if (swagger.IsEnabled)
{
    app.UseMiddleware<SwaggerAuthenticationMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseWhen(context => context.IsApiRequest(), builder =>
{
    //builder.UseAuthentication();
    //builder.UseAuthorization();

    //builder.UseRateLimiter();
});

app.MapEndpoints<GreetingsEndpoint>();

app.MapRazorPages();

app.Run();

public interface IEndpointRouteHandler
{
    static abstract void MapEndpoints(IEndpointRouteBuilder endpoints);
}

public static class IEndpointRouteBuilderExtensions
{
    public static void MapEndpoints<T>(this IEndpointRouteBuilder endpoints)
        where T : IEndpointRouteHandler => T.MapEndpoints(endpoints);
}