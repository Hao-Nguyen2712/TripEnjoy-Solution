using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TripEnjoy.Client.ViewModels;

namespace TripEnjoy.Client.Handlers
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                // Cannot access cookies, proceed without auth
                return await base.SendAsync(request, cancellationToken);
            }

            var accessToken = httpContext.Request.Cookies["accessToken"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = httpContext.Request.Cookies["refreshToken"];
                if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(accessToken))
                {
                    var newTokens = await RefreshTokensAsync(accessToken, refreshToken, cancellationToken);
                    if (newTokens != null)
                    {
                        SetTokenCookies(newTokens);

                        // Clone the original request to resend it
                        var clonedRequest = await CloneRequestAsync(request);
                        clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Token);
                        return await base.SendAsync(clonedRequest, cancellationToken);
                    }
                }

                // If refresh fails, redirect to login
                httpContext.Response.Redirect("/auth/sign-in");
            }

            return response;
        }

        private async Task<AuthResultVM?> RefreshTokensAsync(string expiredAccessToken, string refreshToken, CancellationToken cancellationToken)
        {
            var tokenRequest = new RefreshTokenRequestVM
            {
                ExpiredAccessToken = expiredAccessToken,
                RefreshToken = refreshToken
            };

            // Use a specific HttpClient that doesn't have this handler to avoid an infinite loop
            var client = _httpClientFactory.CreateClient("AuthApiClient");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7199/api/v1/auth/refresh-token");
            request.Content = new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<AuthResultVM>>(responseString);
                return apiResponse?.Data;
            }

            return null;
        }

        private void SetTokenCookies(AuthResultVM tokens)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            context.Response.Cookies.Append("refreshToken", tokens.RefreshToken, cookieOptions);
            context.Response.Cookies.Append("accessToken", tokens.Token, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
        }

        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                foreach (var h in request.Content.Headers)
                {
                    clone.Content.Headers.Add(h.Key, h.Value);
                }
            }
            clone.Version = request.Version;

            foreach (var prop in request.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object>(prop.Key), prop.Value);
            }

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
