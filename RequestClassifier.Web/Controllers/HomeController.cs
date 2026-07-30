using Microsoft.AspNetCore.Mvc;

namespace RequestClassifier.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}