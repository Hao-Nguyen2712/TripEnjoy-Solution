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
            
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register");
            // We specify the role in the payload
            var apiRequestPayload = new
            {
                signUpRequest.Email,
                signUpRequest.Password,
                ConfirmFor = "Partner"
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(apiRequestPayload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please check your email to confirm your account.";
                return RedirectToAction("SignIn");
            }
            else
            {
                // Handle API errors
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // Attempt to deserialize into the standard validation problem details structure
                var validationProblemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(errorContent);
                if (validationProblemDetails != null && validationProblemDetails.Errors.Any())
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
                     // Fallback for other error types
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                }
                
                return View(signUpRequest);
            }
        }

        [Route("sign-in")]
        public IActionResult SignIn()
        {
            return View();
        }
    }
}
