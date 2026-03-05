using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Infrastructure.Data;

namespace MooreHotelAndSuites.API.Middleware
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public AuditLogMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (!context.Request.Path.StartsWithSegments("/api"))
                return;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = context.GetEndpoint()?.DisplayName ?? "Unknown",
                Entity = context.GetEndpoint()?.Metadata
                            .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                            .FirstOrDefault()?.ControllerName ?? "Unknown",
                Path = context.Request.Path.Value ?? string.Empty,
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                OccurredAt = DateTime.UtcNow
            };

            db.AuditLogs.Add(log);
            await db.SaveChangesAsync();
        }
    }
}