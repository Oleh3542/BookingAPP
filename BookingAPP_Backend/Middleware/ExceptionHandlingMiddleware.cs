using System.Net;
using System.Text.Json;
using BookingAPP_Backend.Common;

namespace BookingAPP_Backend.Middleware;

/// Централізована обробка винятків. Гарантує, що клієнт ніколи не побачить

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Ресурс не знайдено"),
            BookingConflictException => (HttpStatusCode.Conflict, "Конфлікт бронювання"),
            ValidationFailedException => (HttpStatusCode.BadRequest, "Помилка валідації"),
            ApiException => (HttpStatusCode.BadRequest, "Помилка запиту"),
            _ => (HttpStatusCode.InternalServerError, "Внутрішня помилка сервера")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            // Логуємо повну помилку на сервері, але не показуємо деталі клієнту
            _logger.LogError(ex, "Необроблений виняток під час обробки {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{Title}: {Message} ({Method} {Path})", title, ex.Message, context.Request.Method, context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            title,
            status = (int)statusCode,
            message = statusCode == HttpStatusCode.InternalServerError && !_env.IsDevelopment()
                ? "Сталася непередбачена помилка. Спробуйте пізніше."
                : ex.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}