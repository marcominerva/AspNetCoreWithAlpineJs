namespace MinimalRazor.Extensions;

public static class RequestExtensions
{
    public static bool IsApiRequest(this HttpContext httpContext)
        => httpContext.Request.Path.StartsWithSegments("/api");

    public static bool IsSwaggerRequest(this HttpContext httpContext)
        => httpContext.Request.Path.StartsWithSegments("/swagger");

    public static bool IsWebRequest(this HttpContext httpContext)
        => !httpContext.IsApiRequest() && !httpContext.IsSwaggerRequest();
}
