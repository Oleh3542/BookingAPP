using Microsoft.AspNetCore.Mvc.Filters;

namespace BookingAPP_Backend.Middleware;

// захист адміністративних 

public class ApiKeyAuthAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var headerName = config["Security:AdminApiKeyHeader"] ?? "X-Api-Key";
        var expectedKey = config["Security:AdminApiKey"];

        if (string.IsNullOrWhiteSpace(expectedKey))
        {
        
            context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new
            {
                title = "Сервер неправильно налаштовано",
                status = 500,
                message = "Адміністративний доступ тимчасово недоступний."
            })
            { StatusCode = 500 };
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(headerName, out var providedKey) ||
            !CryptographicallySafeEquals(providedKey.ToString(), expectedKey))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new
            {
                title = "Не авторизовано",
                status = 401,
                message = $"Для цієї операції потрібен дійсний заголовок '{headerName}'."
            })
            { StatusCode = 401 };
            return;
        }

        base.OnActionExecuting(context);
    }

    private static bool CryptographicallySafeEquals(string a, string b)
    {
        var bytesA = System.Text.Encoding.UTF8.GetBytes(a);
        var bytesB = System.Text.Encoding.UTF8.GetBytes(b);
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}
