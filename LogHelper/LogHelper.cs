using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Threading.Tasks;

namespace InventoryManagementSystem.LogHelper
{
    public class LogHelper
    {
        private readonly RequestDelegate _next;

        public LogHelper(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Anonymous";

            var endpoint = context.GetEndpoint()?.DisplayName ?? context.Request.Path;

            using (LogContext.PushProperty("User", user))
            using (LogContext.PushProperty("Endpoint", endpoint))
            {
                await _next(context);
            }
        }
    }
}