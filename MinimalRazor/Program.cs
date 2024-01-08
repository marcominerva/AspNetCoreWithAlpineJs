using System.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using MinimalRazor.BusinessLayer.Settings;
using MinimalRazor.Endpoints;
using MinimalRazor.ExceptionHandlers;
using MinimalRazor.Extensions;
using MinimalRazor.Swagger;
using TinyHelpers.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var swagger = builder.Services.ConfigureAndGet<SwaggerSettings>(builder.Configuration, nameof(SwaggerSettings));

builder.Services.AddRazorPages();
builder.Services.AddWebOptimizer(minifyCss: true, minifyJavaScript: builder.Environment.IsProduction());

if (swagger.IsEnabled)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services.AddExceptionHandler<DefaultExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var statusCode = context.ProblemDetails.Status.GetValueOrDefault(StatusCodes.Status500InternalServerError);
        context.ProblemDetails.Type ??= $"https://httpstatuses.io/{statusCode}";
        context.ProblemDetails.Title ??= ReasonPhrases.GetReasonPhrase(statusCode);
        context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    };
});

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
    builder.UseExceptionHandler();
    builder.UseStatusCodePages();
});

app.UseWebOptimizer();
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

public interface IEndpointRouteHandlerBuilder
{
    static abstract void MapEndpoints(IEndpointRouteBuilder endpoints);
}

public static class IEndpointRouteBuilderExtensions
{
    public static void MapEndpoints<T>(this IEndpointRouteBuilder endpoints)
        where T : IEndpointRouteHandlerBuilder => T.MapEndpoints(endpoints);
}