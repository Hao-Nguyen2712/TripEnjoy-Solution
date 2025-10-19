using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TripEnjoy.Client.ViewModels;

namespace TripEnjoy.Client.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Authorize]
    public class PropertiesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(IHttpClientFactory httpClientFactory, ILogger<PropertiesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync("/api/v1/properties/mine");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<IEnumerable<PropertyListItemVM>>>(content);
                    
                    if (apiResponse?.Data != null)
                    {
                        return View(apiResponse.Data);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to fetch properties. Status: {StatusCode}", response.StatusCode);
                    ViewBag.ErrorMessage = "Failed to load properties. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching properties");
                ViewBag.ErrorMessage = "An error occurred while loading properties.";
            }

            return View(new List<PropertyListItemVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreatePropertyVM();
            
            try
            {
                // Load property types for dropdown
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync("/api/v1/property-types");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<IEnumerable<PropertyTypeVM>>>(content);
                    
                    if (apiResponse?.Data != null)
                    {
                        model.PropertyTypes = apiResponse.Data.ToList();
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to fetch property types. Status: {StatusCode}", response.StatusCode);
                    ViewBag.ErrorMessage = "Failed to load property types.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching property types");
                ViewBag.ErrorMessage = "An error occurred while loading the form.";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePropertyVM model)
        {
            if (!ModelState.IsValid)
            {
                // Reload property types for dropdown
                await LoadPropertyTypes(model);
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                
                var propertyData = new
                {
                    PropertyTypeId = model.PropertyTypeId,
                    Name = model.Name,
                    Address = model.Address,
                    City = model.City,
                    Country = model.Country,
                    Description = model.Description,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude
                };

                var json = JsonConvert.SerializeObject(propertyData);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/v1/properties", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Property created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create property. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(errorContent);
                    ModelState.AddModelError("", errorResponse?.Message ?? "Failed to create property.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating property");
                ModelState.AddModelError("", "An error occurred while creating the property.");
            }

            // Reload property types for dropdown
            await LoadPropertyTypes(model);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync($"/api/v1/properties/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<PropertyDetailsVM>>(content);
                    
                    if (apiResponse?.Data != null)
                    {
                        return View(apiResponse.Data);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch property details. Status: {StatusCode}", response.StatusCode);
                    ViewBag.ErrorMessage = "Failed to load property details.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching property details for ID: {PropertyId}", id);
                ViewBag.ErrorMessage = "An error occurred while loading property details.";
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                // Get property details
                var propertyResponse = await client.GetAsync($"/api/v1/properties/{id}");
                if (!propertyResponse.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Property not found.";
                    return View();
                }

                var propertyContent = await propertyResponse.Content.ReadAsStringAsync();
                var propertyApiResponse = JsonConvert.DeserializeObject<ApiResponseVM<PropertyDetailsVM>>(propertyContent);

                if (propertyApiResponse?.Data == null)
                {
                    ViewBag.ErrorMessage = "Property not found.";
                    return View();
                }

                // Get property types for dropdown
                var propertyTypesResponse = await client.GetAsync("/api/v1/property-types");
                var propertyTypesContent = await propertyTypesResponse.Content.ReadAsStringAsync();
                var propertyTypesApiResponse = JsonConvert.DeserializeObject<ApiResponseVM<IEnumerable<PropertyTypeVM>>>(propertyTypesContent);

                var property = propertyApiResponse.Data;
                var model = new EditPropertyVM
                {
                    Id = property.Id,
                    PropertyTypeId = property.PropertyTypeId,
                    Name = property.Name,
                    Address = property.Address,
                    City = property.City,
                    Country = property.Country,
                    Description = property.Description,
                    Latitude = property.Latitude,
                    Longitude = property.Longitude,
                    PropertyTypes = propertyTypesApiResponse?.Data?.ToList() ?? new List<PropertyTypeVM>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for property ID: {PropertyId}", id);
                ViewBag.ErrorMessage = "An error occurred while loading the edit form.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPropertyVM model)
        {
            if (!ModelState.IsValid)
            {
                // Reload property types if validation fails
                try
                {
                    var client = _httpClientFactory.CreateClient("ApiClient");
                    var propertyTypesResponse = await client.GetAsync("/api/v1/property-types");
                    var propertyTypesContent = await propertyTypesResponse.Content.ReadAsStringAsync();
                    var propertyTypesApiResponse = JsonConvert.DeserializeObject<ApiResponseVM<IEnumerable<PropertyTypeVM>>>(propertyTypesContent);
                    model.PropertyTypes = propertyTypesApiResponse?.Data?.ToList() ?? new List<PropertyTypeVM>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading property types for edit form");
                }
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                var updateCommand = new
                {
                    PropertyId = model.Id,
                    PropertyTypeId = model.PropertyTypeId,
                    Name = model.Name,
                    Address = model.Address,
                    City = model.City,
                    Country = model.Country,
                    Description = model.Description,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude
                };

                var json = JsonConvert.SerializeObject(updateCommand);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"/api/v1/properties/{model.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Property updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update property. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
                    ModelState.AddModelError("", "Failed to update property. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating property");
                ModelState.AddModelError("", "An error occurred while updating the property.");
            }

            // Reload property types on error
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var propertyTypesResponse = await client.GetAsync("/api/v1/property-types");
                var propertyTypesContent = await propertyTypesResponse.Content.ReadAsStringAsync();
                var propertyTypesApiResponse = JsonConvert.DeserializeObject<ApiResponseVM<IEnumerable<PropertyTypeVM>>>(propertyTypesContent);
                model.PropertyTypes = propertyTypesApiResponse?.Data?.ToList() ?? new List<PropertyTypeVM>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading property types for edit form");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageImages(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync($"/api/v1/properties/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Note: API returns PropertyDto but we're deserializing as PropertyDetailsVM
                    // This works because JSON.NET is flexible, but some fields may not map correctly
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<PropertyDetailsVM>>(content);
                    
                    if (apiResponse?.Data != null)
                    {
                        // Ensure proper mapping of image data
                        var propertyImages = new List<PropertyImageVM>();
                        
                        // Parse the raw JSON to get the actual image data structure
                        var jsonObject = JsonConvert.DeserializeObject<dynamic>(content);
                        var imagesArray = jsonObject?.data?.images;
                        
                        if (imagesArray != null)
                        {
                            for (int i = 0; i < imagesArray.Count; i++)
                            {
                                var imageData = imagesArray[i];
                                propertyImages.Add(new PropertyImageVM
                                {
                                    Id = Guid.Parse(imageData.id.ToString()),
                                    ImageUrl = imageData.imageUrl?.ToString() ?? "",
                                    IsPrimary = bool.Parse(imageData.isMain?.ToString() ?? "false"),
                                    Caption = null, // Not stored in current domain model
                                    DisplayOrder = i + 1, // Generate based on order in array
                                    UploadAt = DateTime.Parse(imageData.uploadAt?.ToString() ?? DateTime.Now.ToString())
                                });
                            }
                        }
                        
                        var viewModel = new ManageImagesVM
                        {
                            PropertyId = apiResponse.Data.Id,
                            PropertyName = apiResponse.Data.Name,
                            Images = propertyImages
                        };
                        return View(viewModel);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch property for image management. Status: {StatusCode}", response.StatusCode);
                    ViewBag.ErrorMessage = "Failed to load property information.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching property for image management. ID: {PropertyId}", id);
                ViewBag.ErrorMessage = "An error occurred while loading the property.";
            }

            return View(new ManageImagesVM { PropertyId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateImageUploadUrl(Guid propertyId, string fileName, string? caption = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                
                var requestData = new
                {
                    FileName = fileName,
                    Caption = caption
                };

                var json = JsonConvert.SerializeObject(requestData);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"/api/v1/properties/{propertyId}/images/upload-url", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Deserialize to proper API response structure
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<PhotoUploadUrlVM>>(content);
                    
                    if (apiResponse?.Data != null)
                    {
                        return Json(new { 
                            success = true, 
                            data = new 
                            {
                                uploadUrl = apiResponse.Data.UploadUrl,
                                uploadParameters = apiResponse.Data.UploadParameters
                            }
                        });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid response structure from API." });
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to generate upload URL. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(errorContent);
                    return Json(new { success = false, message = errorResponse?.Message ?? "Failed to generate upload URL." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating upload URL for property {PropertyId}", propertyId);
                return Json(new { success = false, message = "An error occurred while generating upload URL." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(Guid propertyId, AddImageVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid image data." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                
                var imageData = new
                {
                    PublicId = model.PublicId,
                    ImageUrl = model.ImageUrl,
                    Signature = model.Signature,
                    Timestamp = model.Timestamp,
                    IsCover = model.IsCover,
                    Caption = model.Caption
                };

                var json = JsonConvert.SerializeObject(imageData);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"/api/v1/properties/{propertyId}/images", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Image added successfully!" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to add image. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(errorContent);
                    return Json(new { success = false, message = errorResponse?.Message ?? "Failed to add image." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image to property {PropertyId}", propertyId);
                return Json(new { success = false, message = "An error occurred while adding the image." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(Guid propertyId, Guid imageId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.DeleteAsync($"/api/v1/properties/{propertyId}/images/{imageId}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Image deleted successfully!" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to delete image. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(errorContent);
                    return Json(new { success = false, message = errorResponse?.Message ?? "Failed to delete image." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageId} from property {PropertyId}", imageId, propertyId);
                return Json(new { success = false, message = "An error occurred while deleting the image." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCoverImage(Guid propertyId, Guid imageId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.PutAsync($"/api/v1/properties/{propertyId}/images/{imageId}/set-cover", null);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Cover image updated successfully!" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to set cover image. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponseVM<object>>(errorContent);
                    return Json(new { success = false, message = errorResponse?.Message ?? "Failed to set cover image." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cover image {ImageId} for property {PropertyId}", imageId, propertyId);
                return Json(new { success = false, message = "An error occurred while setting the cover image." });
            }
        }

        private async Task LoadPropertyTypes(CreatePropertyVM model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync("/api/v1/property-types");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseVM<IEnumerable<PropertyTypeVM>>>(content);
                    
                    if (apiResponse?.Data != null)
                    {
                        model.PropertyTypes = apiResponse.Data.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading property types");
                model.PropertyTypes = new List<PropertyTypeVM>();
            }
        }
    }
}