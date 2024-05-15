
using Microsoft.AspNetCore.RateLimiting;
using RateLimitApi;
using System.Net;
using System.Threading.RateLimiting;

namespace RateLimitingApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        var fixedPolicy = "fixed";
        builder.Services.AddRateLimiter(_ => _
            .AddFixedWindowLimiter(policyName: fixedPolicy, options =>
            {
                options.PermitLimit = 4;
                options.Window = TimeSpan.FromSeconds(10);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            }).OnRejected = (context, CancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                return new ValueTask();
            });

        var slidingPolicy = "sliding";
        builder.Services.AddRateLimiter(_ => _
            .AddSlidingWindowLimiter(policyName: slidingPolicy, options =>
            {
                options.PermitLimit = 2;
                options.Window = TimeSpan.FromSeconds(5);
                options.SegmentsPerWindow = 1;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            }));

        var app = builder.Build();
        app.UseRateLimiter();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseMiddleware<RateLimitMiddleware>();
        app.MapControllers();
        app.Run();
    }
}
