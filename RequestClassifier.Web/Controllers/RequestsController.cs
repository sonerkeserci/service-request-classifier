using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RequestClassifier.Application.DTOs.ServiceRequests;

namespace RequestClassifier.Web.Controllers;

public class RequestsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RequestsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateServiceRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var client = _httpClientFactory.CreateClient(
            "RequestClassifierApi");

        var response = await client.PostAsJsonAsync(
            "api/ServiceRequests",
            dto);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Talep oluşturulurken bir hata oluştu.");

            return View(dto);
        }

        var result = await response.Content
            .ReadFromJsonAsync<CreateServiceRequestResultDto>();

        ViewBag.Result = result;

        ModelState.Clear();

        return View(new CreateServiceRequestDto());
    }

    [HttpGet]
    public IActionResult Track()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Track(
        TrackServiceRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var client = _httpClientFactory.CreateClient(
            "RequestClassifierApi");

        var response = await client.PostAsJsonAsync(
            "api/ServiceRequests/track",
            dto);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            ViewBag.NotFound = true;
            return View(dto);
        }

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Talep sorgulanırken bir hata oluştu.");

            return View(dto);
        }

        var result = await response.Content
            .ReadFromJsonAsync<TrackServiceRequestResultDto>();

        ViewBag.Result = result;

        return View(dto);
    }
}