namespace TripEnjoy.Client.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UsePartnerAuthRedirect(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PartnerAuthRedirectMiddleware>();
        }
    }
}