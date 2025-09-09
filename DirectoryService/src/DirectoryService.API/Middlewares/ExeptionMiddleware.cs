using DirectoryService.API.Response;
using DirectoryService.Domain.Shared;

namespace DirectoryService.API.Middlewares;

public class ExeptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExeptionMiddleware> _logger;

    public ExeptionMiddleware(RequestDelegate next, ILogger<ExeptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.Message);

            var error = Error.Failure("internal.server.exception", ex.Message);
            var envelope = Envelope.Error(error);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(envelope);
        }
    }
}

public static class ExeptionMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExeptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExeptionMiddleware>();
    }
}