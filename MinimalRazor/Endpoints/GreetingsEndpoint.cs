using Microsoft.AspNetCore.Http.HttpResults;

namespace MinimalRazor.Endpoints;

public class GreetingsEndpoint : IEndpointRouteHandler
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var greetingsApiGroup = endpoints.MapGroup("/api/greetings");

        greetingsApiGroup.MapPost(string.Empty, MakeGreetings);
    }

    public static Ok<Greetings> MakeGreetings(Person person, ILogger<GreetingsEndpoint> logger)
    {
        var message = $"Hello, {person.Name}!";
        var greetings = new Greetings(message);

        return TypedResults.Ok(greetings);
    }
}

public record class Person(string Name);

public record class Greetings(string Message);
