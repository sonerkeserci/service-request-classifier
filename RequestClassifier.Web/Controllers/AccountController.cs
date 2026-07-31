using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RequestClassifier.Application.DTOs.Auth;

namespace RequestClassifier.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var client = _httpClientFactory.CreateClient(
            "RequestClassifierApi");

        var response = await client.PostAsJsonAsync(
            "api/Auth/login",
            dto);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            ModelState.AddModelError(
                string.Empty,
                "E-posta veya şifre hatalı.");

            return View(dto);
        }

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Giriş sırasında bir hata oluştu.");

            return View(dto);
        }

        var result = await response.Content
            .ReadFromJsonAsync<AuthResponseDto>();

        if (result is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Sunucudan geçerli bir giriş cevabı alınamadı.");

            return View(dto);
        }

        HttpContext.Session.SetString(
            "JwtToken",
            result.Token);

        HttpContext.Session.SetString(
            "UserEmail",
            result.Email);

        HttpContext.Session.SetString(
            "UserRole",
            result.Role);

        HttpContext.Session.SetString(
            "TokenExpiration",
            result.Expiration.ToString("O"));

        if (result.Role == "Admin")
        {
            return RedirectToAction(
                "Index",
                "Panel");
        }

        return RedirectToAction(
            "Requests",
            "Panel");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction(
            "Index",
            "Home");
    }
}