using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TripEnjoy.Client.ViewModels;

namespace TripEnjoy.Client.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Route("partner/auth")]
    public class AuthenController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AuthenController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("sign-up")]
        public IActionResult SignUp()
        {
            return View(new PartnerSignUpRequestVM());
        }

        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> SignUp(PartnerSignUpRequestVM signUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(signUpRequest);
            }

            var client = _clientFactory.CreateClient("ApiClient");

            // Updated to use the new register-partner endpoint
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register-partner");

            // Create payload matching the RegisterPartnerCommand structure
            var apiRequestPayload = new
            {
                signUpRequest.Email,
                signUpRequest.Password,
                signUpRequest.CompanyName,
                signUpRequest.ContactNumber,
                signUpRequest.Address
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(apiRequestPayload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Partner registration successful - skip email verification step
                // Redirect directly to sign-in with success message about immediate access
                TempData["SuccessMessage"] = "Registration successful! You can now sign in and start uploading your business documents for verification.";
                return RedirectToAction("SignIn");
            }
            else
            {
                // Handle API errors
                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    // Try to deserialize as API response with validation errors
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(errorContent);
                    if (apiResponse?.Errors != null)
                    {
                        var errors = JsonConvert.DeserializeObject<List<ApiErrorDetail>>(apiResponse.Errors.ToString());
                        if (errors != null)
                        {
                            foreach (var error in errors)
                            {
                                ModelState.AddModelError(error.Field ?? string.Empty, error.Detail);
                            }
                        }
                    }
                    else
                    {
                        // Try validation problem details format
                        var validationProblemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(errorContent);
                        if (validationProblemDetails?.Errors != null)
                        {
                            foreach (var errorPair in validationProblemDetails.Errors)
                            {
                                foreach (var errorMessage in errorPair.Value)
                                {
                                    ModelState.AddModelError(errorPair.Key, errorMessage);
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                        }
                    }
                }
                catch (JsonException)
                {
                    // Fallback for other error types
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                }

                return View(signUpRequest);
            }
        }

        [Route("sign-in")]
        public IActionResult SignIn(string? returnUrl = null)
        {
            var model = new LoginRequestVM();
            if (TempData["LoginEmail"] is string email)
            {
                model.Email = email;
            }
            
            // Store return URL for after successful login
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }
            
            return View(model);
        }

        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> SignIn(LoginRequestVM loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(loginRequest);
            }

            var client = _clientFactory.CreateClient("ApiClient");

            // Step 1: Call login-step-one (same for both users and partners)
            var stepOneRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login-partner-step-one");
            stepOneRequest.Content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            var stepOneResponse = await client.SendAsync(stepOneRequest);

            if (stepOneResponse.IsSuccessStatusCode)
            {
                // Store email in TempData for OTP verification
                TempData["Email"] = loginRequest.Email;
                TempData["IsPartner"] = true; // Flag to identify partner login
                
                // Preserve return URL through the OTP verification process
                if (TempData["ReturnUrl"] is string returnUrl)
                {
                    TempData.Keep("ReturnUrl");
                }
                
                return RedirectToAction("VerifyOtp");
            }
            else
            {
                // Handle login failure
                var errorContent = await stepOneResponse.Content.ReadAsStringAsync();
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(errorContent);
                    if (apiResponse?.Errors != null)
                    {
                        var errors = JsonConvert.DeserializeObject<List<ApiErrorDetail>>(apiResponse.Errors.ToString());
                        if (errors != null)
                        {
                            foreach (var error in errors)
                            {
                                ModelState.AddModelError(error.Field ?? string.Empty, error.Detail);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                }

                TempData["LoginEmail"] = loginRequest.Email; // Preserve email on failure
                return View(loginRequest);
            }
        }

        [Route("verify-otp")]
        public IActionResult VerifyOtp()
        {
            var email = TempData["Email"] as string;
            if (string.IsNullOrEmpty(email))
            {
                // If email is not in TempData, redirect to sign-in
                return RedirectToAction("SignIn");
            }

            // Preserve return URL for the POST action
            if (TempData["ReturnUrl"] is string returnUrl)
            {
                TempData.Keep("ReturnUrl");
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

            // Step 2: Call login-step-two (same for both users and partners)
            var stepTwoRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login-step-two");
            stepTwoRequest.Content = new StringContent(JsonConvert.SerializeObject(verifyOtpRequest), Encoding.UTF8, "application/json");

            var stepTwoResponse = await client.SendAsync(stepTwoRequest);

            if (stepTwoResponse.IsSuccessStatusCode)
            {
                var responseString = await stepTwoResponse.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<AuthResultVM>>(responseString);

                if (apiResponse?.Data != null)
                {
                    // Parse JWT token and create authentication
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(apiResponse.Data.Token);
                    var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
                        jwtToken.Claims,
                        Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.Add(jwtToken.ValidTo - DateTime.UtcNow)
                    };

                    await HttpContext.SignInAsync(
                        Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                        new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Set refresh token cookie
                    var refreshTokenCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddDays(7),
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };
                    Response.Cookies.Append("refreshToken", apiResponse.Data.RefreshToken, refreshTokenCookieOptions);

                    // Set access token cookie
                    var accessTokenCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = authProperties.ExpiresUtc
                    };
                    Response.Cookies.Append("accessToken", apiResponse.Data.Token, accessTokenCookieOptions);

                    TempData["ClearOtpTimer"] = true;

                    // Check if there's a return URL to redirect to
                    if (TempData["ReturnUrl"] is string returnUrl && 
                        !string.IsNullOrEmpty(returnUrl) && 
                        returnUrl.StartsWith("/partner", StringComparison.OrdinalIgnoreCase))
                    {
                        return Redirect(returnUrl);
                    }

                    // Default redirect to partner dashboard
                    return RedirectToAction("Index", "Home", new { area = "Partner" });
                }
            }

            // Handle OTP verification failure
            ModelState.AddModelError(string.Empty, "Invalid OTP. Please try again.");
            return View(verifyOtpRequest);
        }
    }

    // Helper class for validation problem details
    public class ValidationProblemDetails
    {
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}