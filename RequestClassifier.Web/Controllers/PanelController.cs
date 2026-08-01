using Microsoft.AspNetCore.Mvc;
using RequestClassifier.Application.DTOs.Departments;
using RequestClassifier.Application.DTOs.RequestCategories;
using RequestClassifier.Application.DTOs.ServiceRequests;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace RequestClassifier.Web.Controllers;

public class PanelController : Controller
{
    // Used to create HttpClient instances.
    private readonly IHttpClientFactory _httpClientFactory;

    // Provides access to configuration values such as API BaseUrl.
    private readonly IConfiguration _configuration;

    public PanelController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    [HttpGet]
    public IActionResult Index()
    {
        var loginRedirect = ValidateSession();

        if (loginRedirect is not null)
        {
            return loginRedirect;
        }

        // Employees do not have access to the dashboard.
        if (!IsAdmin())
        {
            return RedirectToAction(nameof(Requests));
        }

        return View();
    }

    [HttpGet]
    public IActionResult Requests()
    {
        var loginRedirect = ValidateSession();

        if (loginRedirect is not null)
        {
            return loginRedirect;
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetRequests()
    {
        var loginRedirect = ValidateSession();

        if (loginRedirect is not null)
        {
            return Unauthorized();
        }

        // Get the JWT token stored during login.
        var token =
            HttpContext.Session.GetString("JwtToken");

        // Read the API base address from appsettings.json.
        var apiBaseUrl =
            _configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "API adresi yapılandırılmamış."
                });
        }

        // Create a managed HttpClient instance.
        var client =
            _httpClientFactory.CreateClient();

        client.BaseAddress =
            new Uri(apiBaseUrl);

        // Add the JWT token to the Authorization header.
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            /*
             * Calls the protected API endpoint.
             *
             * Replace this route if your actual endpoint
             * uses a different address.
             */
            var response =
                await client.GetAsync(
                    "api/ServiceRequests");

            // The API rejected the JWT token.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();

                return Unauthorized();
            }

            // The authenticated user does not have permission.
            if (response.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu işlem için yetkiniz bulunmuyor."
                    });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Talepler API üzerinden alınamadı.",

                        detail = apiError
                    });
            }

            /*
             * Return the API response directly to JavaScript.
             * The JSON property names are preserved.
             */
            var json =
                await response.Content.ReadAsStringAsync();

            return Content(
                json,
                "application/json");
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestDetail(int id)
    {
        var loginRedirect = ValidateSession();

        if (loginRedirect is not null)
        {
            return Unauthorized();
        }

        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Geçerli bir talep ID değeri gönderilmedi."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            // Admin can access every request.
            // Employee access is filtered by the API according
            // to the departmentId claim stored in the JWT.
            var response = await client.GetAsync(
                $"api/ServiceRequests/{id}");

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return Unauthorized();
            }

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu talebi görüntüleme yetkiniz bulunmuyor."
                    });
            }

            if (response.StatusCode ==
                System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(new
                {
                    message =
                        "Talep bulunamadı veya bu talebe erişim yetkiniz yok."
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Talep detayı API üzerinden alınamadı.",

                        detail = apiError
                    });
            }

            var json =
                await response.Content.ReadAsStringAsync();

            return Content(
                json,
                "application/json");
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }

    [HttpPut]
  public async Task<IActionResult> UpdateRequestStatus(
    int id,
    [FromBody] UpdateRequestStatusDto dto)
    {
        var loginRedirect = ValidateSession();

        if (loginRedirect is not null)
        {
            return Unauthorized();
        }

        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Geçerli bir talep ID değeri gönderilmedi."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            // Sends the new status and description to the protected API endpoint.
            var response = await client.PutAsJsonAsync(
                $"api/ServiceRequests/{id}/status",
                dto);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();

                return Unauthorized();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu talebin durumunu güncelleme yetkiniz bulunmuyor."
                    });
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new
                {
                    message =
                        "Talep bulunamadı veya bu talebe erişim yetkiniz yok."
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Talep durumu API üzerinden güncellenemedi.",

                        detail = apiError
                    });
            }

            return NoContent();
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequestHistory(int id)
    {
        var loginRedirect = ValidateSession();

        if (loginRedirect is not null)
        {
            return Unauthorized();
        }

        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Geçerli bir talep ID değeri gönderilmedi."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            // Employee access is restricted by the departmentId
            // claim inside the API service.
            var response = await client.GetAsync(
                $"api/ServiceRequests/{id}/histories");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return Unauthorized();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu talebin geçmişini görüntüleme yetkiniz bulunmuyor."
                    });
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new
                {
                    message =
                        "Talep bulunamadı veya bu talebe erişim yetkiniz yok."
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Durum geçmişi API üzerinden alınamadı.",

                        detail = apiError
                    });
            }

            var json =
                await response.Content.ReadAsStringAsync();

            return Content(
                json,
                "application/json");
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPredictionCandidates(int id)
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "Bu işlem yalnızca yöneticiler tarafından yapılabilir."
                });
        }

        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Geçerli bir talep ID değeri gönderilmedi."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            var response = await client.GetAsync(
                $"api/ServiceRequests/{id}/prediction-candidates");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return Unauthorized();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu işlem için yönetici yetkisi gereklidir."
                    });
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new
                {
                    message =
                        "Talep veya tahmin adayları bulunamadı."
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Tahmin adayları API üzerinden alınamadı.",

                        detail = apiError
                    });
            }

            var json =
                await response.Content.ReadAsStringAsync();

            return Content(
                json,
                "application/json");
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }

    [HttpPut]
    public async Task<IActionResult> AssignRequestCategory(
        int id,
        [FromBody] AssignCategoryDto dto)
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "Bu işlem yalnızca yöneticiler tarafından yapılabilir."
                });
        }

        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Geçerli bir talep ID değeri gönderilmedi."
            });
        }

        if (dto.CategoryId <= 0)
        {
            return BadRequest(new
            {
                message = "Geçerli bir kategori seçilmedi."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            var response = await client.PutAsJsonAsync(
                $"api/ServiceRequests/{id}/assign",
                dto);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                return Unauthorized();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu işlem için yönetici yetkisi gereklidir."
                    });
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(new
                {
                    message =
                        "Talep veya seçilen kategori geçersiz."
                });
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new
                {
                    message =
                        "Talep bulunamadı."
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Talep seçilen kategoriye atanamadı.",

                        detail = apiError
                    });
            }

            return NoContent();
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }
    
    [HttpGet]
    public IActionResult Departments()
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return accessRedirect;
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments()
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return Unauthorized();
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await client.GetAsync(
                "api/Departments");

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode);
        }

        var json =
            await response.Content.ReadAsStringAsync();

        return Content(
            json,
            "application/json");
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment(
    [FromBody] CreateDepartmentDto dto)
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Code))
        {
            return BadRequest(new
            {
                message = "Departman adı ve kodu zorunludur."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.PostAsJsonAsync(
            "api/Departments",
            dto);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();
            return Unauthorized();
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return Forbid();
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail =
                await response.Content.ReadAsStringAsync();

            return StatusCode(
                (int)response.StatusCode,
                new
                {
                    message = "Departman oluşturulamadı.",
                    detail
                });
        }

        var json =
            await response.Content.ReadAsStringAsync();

        return Content(
            json,
            "application/json");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDepartment(
        int id,
        [FromBody] UpdateDepartmentDto dto)
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return Unauthorized();
        }

        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Geçerli bir departman ID değeri gönderilmedi."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Code))
        {
            return BadRequest(new
            {
                message = "Departman adı ve kodu zorunludur."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.PutAsJsonAsync(
            $"api/Departments/{id}",
            dto);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();
            return Unauthorized();
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return Forbid();
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail =
                await response.Content.ReadAsStringAsync();

            return StatusCode(
                (int)response.StatusCode,
                new
                {
                    message = "Departman güncellenemedi.",
                    detail
                });
        }

        return NoContent();
    }


    [HttpGet]
    public IActionResult Categories()
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return accessRedirect;
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return Unauthorized();
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            var response =
                await client.GetAsync(
                    "api/RequestCategories");

            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();

                return Unauthorized();
            }

            if (response.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu işlem için yönetici yetkisi gereklidir."
                    });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Kategoriler API üzerinden alınamadı.",

                        detail = apiError
                    });
            }

            var json =
                await response.Content.ReadAsStringAsync();

            return Content(
                json,
                "application/json");
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateRequestCategoryDto dto)
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return Unauthorized();
        }

        if (dto is null)
        {
            return BadRequest(new
            {
                message =
                    "Kategori bilgileri gönderilmedi."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Code) ||
            dto.DepartmentId <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Kategori adı, kodu ve departman zorunludur."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            var response =
                await client.PostAsJsonAsync(
                    "api/RequestCategories",
                    dto);

            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();

                return Unauthorized();
            }

            if (response.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu işlem için yönetici yetkisi gereklidir."
                    });
            }

            if (response.StatusCode ==
                HttpStatusCode.BadRequest)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return BadRequest(new
                {
                    message =
                        "Kategori oluşturulamadı.",

                    detail = apiError
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Kategori API üzerinden oluşturulamadı.",

                        detail = apiError
                    });
            }

            var json =
                await response.Content.ReadAsStringAsync();

            return Content(
                json,
                "application/json");
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] UpdateRequestCategoryDto dto)
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return Unauthorized();
        }

        if (id <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Geçerli bir kategori ID değeri gönderilmedi."
            });
        }

        if (dto is null)
        {
            return BadRequest(new
            {
                message =
                    "Kategori bilgileri gönderilmedi."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Code) ||
            dto.DepartmentId <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Kategori adı, kodu ve departman zorunludur."
            });
        }

        var token =
            HttpContext.Session.GetString("JwtToken");

        var client =
            _httpClientFactory.CreateClient(
                "RequestClassifierApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        try
        {
            var response =
                await client.PutAsJsonAsync(
                    $"api/RequestCategories/{id}",
                    dto);

            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();

                return Unauthorized();
            }

            if (response.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Bu işlem için yönetici yetkisi gereklidir."
                    });
            }

            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return NotFound(new
                {
                    message =
                        "Kategori bulunamadı."
                });
            }

            if (response.StatusCode ==
                HttpStatusCode.BadRequest)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return BadRequest(new
                {
                    message =
                        "Kategori güncellenemedi.",

                    detail = apiError
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message =
                            "Kategori API üzerinden güncellenemedi.",

                        detail = apiError
                    });
            }

            return NoContent();
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "API servisine ulaşılamıyor."
                });
        }
    }

    [HttpGet]
    public IActionResult Employees()
    {
        var accessRedirect = ValidateAdminAccess();

        if (accessRedirect is not null)
        {
            return accessRedirect;
        }

        return View();
    }

    private IActionResult? ValidateAdminAccess()
    {
        var loginRedirect = ValidateSession();

        if (loginRedirect is not null)
        {
            return loginRedirect;
        }

        if (!IsAdmin())
        {
            return RedirectToAction(nameof(Requests));
        }

        return null;
    }

    private IActionResult? ValidateSession()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        var expirationText =
            HttpContext.Session.GetString("TokenExpiration");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(
                "Login",
                "Account");
        }

        var expirationIsValid = DateTimeOffset.TryParse(
            expirationText,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var expiration);

        if (!expirationIsValid ||
            expiration <= DateTimeOffset.UtcNow)
        {
            HttpContext.Session.Clear();

            TempData["LoginMessage"] =
                "Oturum süreniz doldu. Lütfen tekrar giriş yapın.";

            return RedirectToAction(
                "Login",
                "Account");
        }

        return null;
    }

    private bool IsAdmin()
    {
        var role =
            HttpContext.Session.GetString("UserRole");

        return role == "Admin";
    }
}