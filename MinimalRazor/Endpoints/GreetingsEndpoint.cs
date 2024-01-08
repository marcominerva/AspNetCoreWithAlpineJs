using Microsoft.AspNetCore.Http.HttpResults;

namespace MinimalRazor.Endpoints;

public class GreetingsEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var greetingsApiGroup = endpoints.MapGroup("/api/greetings");

        greetingsApiGroup.MapPost(string.Empty, MakeGreetingsAsync);
    }

    public static async Task<Ok<Greetings>> MakeGreetingsAsync(Person person, ILogger<GreetingsEndpoint> logger)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));

        var message = $"Hello, {person.Name}!";
        var greetings = new Greetings(message);

        return TypedResults.Ok(greetings);
    }
}

public record class Person(string Name);

public record class Greetings(string Message);
