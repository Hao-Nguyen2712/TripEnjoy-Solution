using Microsoft.AspNetCore.Authentication;

namespace TripEnjoy.Client.Middleware
{
    public class PartnerAuthRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PartnerAuthRedirectMiddleware> _logger;

        public PartnerAuthRedirectMiddleware(RequestDelegate next, ILogger<PartnerAuthRedirectMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request path starts with /partner
            if (context.Request.Path.StartsWithSegments("/partner", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Processing partner route: {Path}", context.Request.Path);

                // Exclude partner auth endpoints and static files from authentication check
                if (context.Request.Path.StartsWithSegments("/partner/auth", StringComparison.OrdinalIgnoreCase) ||
                    IsStaticFile(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                // Check if user is authenticated
                if (context.User.Identity?.IsAuthenticated != true)
                {
                    _logger.LogInformation("Unauthenticated user accessing partner route: {Path}, redirecting to partner sign-in", context.Request.Path);
                    
                    // Store the original URL for redirect after login
                    var returnUrl = context.Request.Path + context.Request.QueryString;
                    var redirectUrl = $"/partner/auth/sign-in?returnUrl={Uri.EscapeDataString(returnUrl)}";
                    
                    context.Response.Redirect(redirectUrl);
                    return;
                }

                // Check if user has Partner role
                var roleClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                if (roleClaim?.Value != "Partner")
                {
                    _logger.LogWarning("User {UserId} with role {Role} attempting to access partner route: {Path}", 
                        context.User.Identity.Name, roleClaim?.Value, context.Request.Path);
                    
                    // User is authenticated but not a partner, show access denied or redirect to partner sign-in
                    context.Response.Redirect("/partner/auth/sign-in?error=access_denied");
                    return;
                }

                _logger.LogDebug("Authorized partner user accessing: {Path}", context.Request.Path);
            }

            await _next(context);
        }

        private static bool IsStaticFile(PathString path)
        {
            var extension = Path.GetExtension(path.Value);
            return !string.IsNullOrEmpty(extension) && 
                   (extension.Equals(".css", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".js", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".ico", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".svg", StringComparison.OrdinalIgnoreCase));
        }
    }
}