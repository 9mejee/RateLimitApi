using System.Collections.Concurrent;

namespace RateLimitApi;

public class RateLimitMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private readonly ConcurrentDictionary<string, RequestTracker> _requestCounts = new ConcurrentDictionary<string, RequestTracker>();

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrEmpty(ipAddress))
        {
            var expireTime = DateTime.UtcNow.AddSeconds(10).TimeOfDay;
            short requestLimit = 5;
            if (_requestCounts.TryGetValue(ipAddress, out var requestTracker) && requestTracker.Count > requestLimit)
            {
                if (requestTracker.Time < DateTime.UtcNow.TimeOfDay)
                {
                    _requestCounts.Remove(ipAddress, out var request);
                }
                else
                {
                    context.Response.Headers.Add("Retry-After", "10");
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                    _requestCounts.AddOrUpdate(ipAddress, new RequestTracker { Count = 2, Time = expireTime }, (_, request) => new RequestTracker { Count = request.Count, Time = expireTime });
                    return;
                }
            }
            _requestCounts.AddOrUpdate(ipAddress, new RequestTracker { Count = 2, Time = expireTime }, (_, request) => new RequestTracker { Count = ++request.Count, Time = expireTime });
        }

        await _next(context);
    }
}