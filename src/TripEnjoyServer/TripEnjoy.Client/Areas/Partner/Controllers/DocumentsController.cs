using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TripEnjoy.Client.ViewModels;

namespace TripEnjoy.Client.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Route("partner/documents")]
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IHttpClientFactory httpClientFactory, ILogger<DocumentsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        [Route("upload")]
        public IActionResult Upload()
        {
            var model = new UploadDocumentVM();
            ViewBag.DocumentTypes = GetDocumentTypes();
            return View(model);
        }

        [HttpPost]
        [Route("generateuploadurl")]
        public async Task<IActionResult> GenerateUploadUrl([FromBody] GenerateUploadUrlRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                var requestData = new
                {
                    documentType = request.DocumentType,
                    fileName = request.FileName
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/v1/partner/documents/upload-url", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<DocumentUploadUrlVM>>(responseContent);
                    return Json(new { success = true, data = apiResponse?.Data });
                }
                else
                {
                    _logger.LogWarning("API call failed. Status: {StatusCode}, Response: {ResponseContent}",
                        response.StatusCode, responseContent);

                    // Check if it's HTML response (indicating redirect to login or error page)
                    if (responseContent.TrimStart().StartsWith("<!DOCTYPE") || responseContent.TrimStart().StartsWith("<html"))
                    {
                        return Json(new { success = false, message = "Authentication failed. Please sign in again." });
                    }

                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(responseContent);
                    var errorMessages = ExtractErrorMessages(errorResponse?.Errors);
                    return Json(new { success = false, message = string.Join(", ", errorMessages) });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating upload URL for document type: {DocumentType}, Response: {ResponseContent}",
                    request.DocumentType, ex.Message);
                return Json(new { success = false, message = $"An error occurred while generating upload URL: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("confirmupload")]
        public async Task<IActionResult> ConfirmUpload([FromBody] ConfirmDocumentUploadVM model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending confirm upload request for {DocumentType} with data: {RequestJson}",
                    model.DocumentType, json);

                var response = await client.PostAsync("api/v1/partner/documents", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Received API response. Status: {StatusCode}, Content: {ResponseContent}",
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<PartnerDocumentIdVM>>(responseContent);
                    return Json(new { success = true, documentId = apiResponse?.Data?.Id });
                }
                else
                {
                    _logger.LogWarning("API call failed. Status: {StatusCode}, Response: {ResponseContent}",
                        response.StatusCode, responseContent);

                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(responseContent);
                    var errorMessages = ExtractErrorMessages(errorResponse?.Errors);
                    return Json(new { success = false, message = string.Join(", ", errorMessages) });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming document upload for type: {DocumentType}", model.DocumentType);
                return Json(new { success = false, message = "An error occurred while confirming document upload." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> List(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync($"api/v1/partner/documents?pageNumber={pageNumber}&pageSize={pageSize}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<PagedListVM<PartnerDocumentVM>>>(responseContent);
                    return View(apiResponse?.Data ?? new PagedListVM<PartnerDocumentVM>());
                }
                else
                {
                    _logger.LogWarning("API call failed. Status: {StatusCode}", response.StatusCode);
                    return View(new PagedListVM<PartnerDocumentVM>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading partner documents");
                return View(new PagedListVM<PartnerDocumentVM>());
            }
        }

        [HttpGet]
        [Route("api")]
        public async Task<IActionResult> GetDocumentsApi(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync($"api/v1/partner/documents?pageNumber={pageNumber}&pageSize={pageSize}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<PagedListVM<PartnerDocumentVM>>>(responseContent);
                    return Json(new { success = true, data = apiResponse?.Data });
                }
                else
                {
                    _logger.LogWarning("API call failed. Status: {StatusCode}, Response: {ResponseContent}",
                        response.StatusCode, responseContent);

                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(responseContent);
                    var errorMessages = ExtractErrorMessages(errorResponse?.Errors);
                    return Json(new { success = false, message = string.Join(", ", errorMessages) });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading partner documents via API");
                return Json(new { success = false, message = "An error occurred while loading documents." });
            }
        }

        private static Dictionary<string, string> GetDocumentTypes()
        {
            return new Dictionary<string, string>
            {
                { "BusinessLicense", "Business License" },
                { "TaxIdentification", "Tax Identification" },
                { "ProofOfAddress", "Proof of Address" },
                { "CompanyRegistration", "Company Registration" },
                { "BankStatement", "Bank Statement" },
                { "IdentityDocument", "Identity Document" }
            };
        }

        private static List<string> ExtractErrorMessages(object? errors)
        {
            var messages = new List<string>();

            if (errors == null) return messages;

            try
            {
                var errorList = JsonConvert.DeserializeObject<List<ApiErrorDetail>>(errors.ToString() ?? "");
                if (errorList != null)
                {
                    messages.AddRange(errorList.Select(e => e.Detail));
                }
            }
            catch
            {
                messages.Add("An error occurred while processing your request.");
            }

            return messages;
        }
    }

    public class GenerateUploadUrlRequest
    {
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}