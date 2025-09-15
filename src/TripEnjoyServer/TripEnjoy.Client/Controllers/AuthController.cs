using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using TripEnjoy.Client.ViewModels;
using System.Security.Claims;

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

            return View(new LoginRequestVM());
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
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login-step-one");
            request.Content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Store email in TempData to use on the next request
                TempData["Email"] = loginRequest.Email;
                return RedirectToAction("VerifyOtp", "Auth");
            }
            
            // Handle login failure
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Index", loginRequest);
        }

        [Route("sign-up")]
        public IActionResult SignUp()
        {
            return View(new SignUpRequestVM());
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

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register");
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
                return RedirectToAction("SignUp");
            }
        }

        [Route("sign-up-confirmation")]
        public IActionResult SignUpConfirmation()
        {
            ViewBag.Email = TempData["Email"];
            return View();
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
            verifyOtpRequest.RememberMe = TempData.Peek("RememberMe") as bool? ?? false;

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
                        IsPersistent = verifyOtpRequest.RememberMe, // Use RememberMe from OTP step if needed, or pass from login
                        ExpiresUtc = DateTimeOffset.UtcNow.Add(jwtToken.ValidTo - DateTime.UtcNow)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);


                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddDays(7),
                        Secure = true, // Ensure this is true for production
                        SameSite = SameSiteMode.Strict
                    };
                    Response.Cookies.Append("refreshToken", apiResponse.Data.RefreshToken, cookieOptions);
                    
                    // Access token might be stored in a session cookie or handled in memory by JS
                    Response.Cookies.Append("accessToken", apiResponse.Data.Token, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });

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
}
