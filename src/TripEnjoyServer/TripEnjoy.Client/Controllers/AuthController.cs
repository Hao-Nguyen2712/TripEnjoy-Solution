using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TripEnjoy.Client.ViewModels;

namespace TripEnjoy.Client.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public AuthController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        [Route("sign-in")]
        public IActionResult Index()
        {

            var model = new LoginRequestVM();
            if (TempData["LoginEmail"] is string email)
            {
                model.Email = email;
            }
            return View(model);
        }

        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> Login(LoginRequestVM loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", loginRequest);
            }

            var client = _clientFactory.CreateClient("ApiClient");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login-user-step-one");
            request.Content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Store email in TempData to use on the next request
                TempData["Email"] = loginRequest.Email;
                return RedirectToAction("VerifyOtp", "Auth");
            }

            // Handle login failure
            var errorMessages = new List<string> { "Invalid login attempt." };
            TempData["ErrorMessages"] = JsonConvert.SerializeObject(errorMessages);
            TempData["LoginEmail"] = loginRequest.Email;
            return RedirectToAction("Index");
        }

        [Route("sign-up")]
        public IActionResult SignUp()
        {
            var model = new SignUpRequestVM();
            if (TempData["SignUpEmail"] is string email)
            {
                model.Email = email;
            }
            return View(model);
        }

        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> SignUp(SignUpRequestVM signUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(signUpRequest);
            }

            var client = _clientFactory.CreateClient("ApiClient");
            var apiRequestPayload = new
            {
                signUpRequest.Email,
                signUpRequest.Password,
                ConfirmFor = "User"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register-user");
            request.Content = new StringContent(JsonConvert.SerializeObject(apiRequestPayload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Email"] = signUpRequest.Email;
                return RedirectToAction("SignUpConfirmation");
            }
            else
            {
                var errorResponseString = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(errorResponseString);

                var errorMessages = new List<string>();
                if (errorResponse?.Errors != null)
                {
                    var errors = JsonConvert.DeserializeObject<List<ApiErrorDetail>>(errorResponse.Errors.ToString());
                    errorMessages.AddRange(errors.Select(e => e.Detail));
                }
                else
                {
                    errorMessages.Add("An unexpected error occurred. Please try again.");
                }

                TempData["ErrorMessages"] = JsonConvert.SerializeObject(errorMessages);
                TempData["SignUpEmail"] = signUpRequest.Email; // Preserve email on failure
                return RedirectToAction("SignUp");
            }
        }

        [Route("sign-up-confirmation")]
        public IActionResult SignUpConfirmation()
        {
            ViewBag.Email = TempData["Email"];
            return View();
        }

        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordRequestVM());
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestVM forgotPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(forgotPasswordRequest);
            }

            var client = _clientFactory.CreateClient("ApiClient");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/forgot-password");
            request.Content = new StringContent(JsonConvert.SerializeObject(forgotPasswordRequest), Encoding.UTF8, "application/json");

            await client.SendAsync(request);

            // Regardless of success or failure, we show the same page to prevent email enumeration
            TempData["Email"] = forgotPasswordRequest.Email;
            return RedirectToAction("VerifyPasswordResetOtp");
        }

        [HttpGet("verify-password-reset-otp")]
        public IActionResult VerifyPasswordResetOtp()
        {
            var email = TempData["Email"] as string;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            var model = new VerifyPasswordResetOtpVM { Email = email };
            return View(model);
        }

        [HttpPost("verify-password-reset-otp")]
        public async Task<IActionResult> VerifyPasswordResetOtp(VerifyPasswordResetOtpVM verifyOtpRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(verifyOtpRequest);
            }

            var client = _clientFactory.CreateClient("ApiClient");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/verify-password-reset-otp");
            request.Content = new StringContent(JsonConvert.SerializeObject(verifyOtpRequest), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<string>>(responseString);
                var resetToken = apiResponse.Data;

                // Pass email and the single-use token to the final reset page
                TempData["Email"] = verifyOtpRequest.Email;
                TempData["ResetToken"] = resetToken;
                return RedirectToAction("ResetPassword");
            }
            else
            {
                var errorMessages = new List<string> { "Invalid OTP. Please try again." };
                TempData["ErrorMessages"] = JsonConvert.SerializeObject(errorMessages);
                TempData["Email"] = verifyOtpRequest.Email; // Keep email for the reloaded page
                return RedirectToAction("VerifyPasswordResetOtp");
            }
        }

        [HttpPost]
        [Route("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var client = _clientFactory.CreateClient("ApiClient");
                var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/auth/logout?refreshToken={refreshToken}");
                await client.SendAsync(request);
            }

            // Always clear cookies regardless of API call success
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("verify-otp")]
        public IActionResult VerifyOtp()
        {
            var email = TempData["Email"] as string;
            if (string.IsNullOrEmpty(email))
            {
                // If email is not in TempData, maybe the user refreshed the page or came directly.
                // Redirect them to the login page.
                return RedirectToAction("Index", "Auth");
            }

            var model = new VerifyOtpRequestVM { Email = email };
            return View(model);
        }

        [HttpPost]
        [Route("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequestVM verifyOtpRequest)
        {

            if (!ModelState.IsValid)
            {
                // Re-populate email if model state is invalid
                verifyOtpRequest.Email ??= TempData["Email"] as string;
                return View(verifyOtpRequest);
            }

            var client = _clientFactory.CreateClient("ApiClient");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login-step-two");
            request.Content = new StringContent(JsonConvert.SerializeObject(verifyOtpRequest), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<AuthResultVM>>(responseString);

                if (apiResponse != null && apiResponse.Data != null)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(apiResponse.Data.Token);
                    var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        // For now, we will make all sessions persistent to solve the restart issue.
                        // "Remember Me" checkbox can be wired up to this property later.
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.Add(jwtToken.ValidTo - DateTime.UtcNow)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);


                    var refreshTokenCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddDays(7),
                        Secure = true, // Ensure this is true for production
                        SameSite = SameSiteMode.Strict
                    };
                    Response.Cookies.Append("refreshToken", apiResponse.Data.RefreshToken, refreshTokenCookieOptions);

                    var accessTokenCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        // Make accessToken persistent to match the authentication cookie
                        Expires = authProperties.ExpiresUtc
                    };
                    Response.Cookies.Append("accessToken", apiResponse.Data.Token, accessTokenCookieOptions);

                    TempData["ClearOtpTimer"] = true;
                    return RedirectToAction("Index", "Home");
                }
            }

            // Handle API error (e.g., wrong OTP)
            var errorMessages = new List<string> { "Invalid OTP. Please try again." };
            // You can also add more specific errors from the API response if available
            TempData["ErrorMessages"] = JsonConvert.SerializeObject(errorMessages);
            return View(verifyOtpRequest);
        }

    }
}

namespace TripEnjoy.Client.ViewModels
{
    public class ApiErrorDetail
    {
        public string Code { get; set; }
        public string Detail { get; set; }
        public string Field { get; set; }
    }

    public class ResendOtpRequestVM
    {
        public string Email { get; set; }
    }
}
