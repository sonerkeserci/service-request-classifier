using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace RequestClassifier.Web.Controllers;

public class PanelController : Controller
{
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