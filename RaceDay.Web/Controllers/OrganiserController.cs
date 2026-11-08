using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models.DTOs;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;
using System.Net.Http.Headers;
using System.Globalization;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;
    public OrganiserController(RaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Dashboard()
    {
        var result = await _api.GetAsync<List<EventDto>>("events");
        var myUserId = HttpContext.Session.GetInt32("UserId");
        var myEvents = result.Data?.Where(e => e.OrganiserID == myUserId).ToList() ?? new List<EventDto>();
        return View(myEvents);
    }

    [HttpGet] public IActionResult Create() => View(new EventFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(EventFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(vm.EventName), "EventName");
        content.Add(new StringContent(vm.Description ?? ""), "Description");

        // Format date perfectly for the API
        content.Add(new StringContent(vm.EventDate.ToString("yyyy-MM-ddTHH:mm:ss")), "EventDate");

        content.Add(new StringContent(vm.Location), "Location");

        // Format decimal with InvariantCulture so it doesn't break
        content.Add(new StringContent(vm.Distance.ToString(CultureInfo.InvariantCulture)), "Distance");

        content.Add(new StringContent(vm.EventType), "EventType");

        if (vm.BannerImage != null && vm.BannerImage.Length > 0)
        {
            var stream = new StreamContent(vm.BannerImage.OpenReadStream());
            stream.Headers.ContentType = new MediaTypeHeaderValue(vm.BannerImage.ContentType);
            content.Add(stream, "BannerImage", vm.BannerImage.FileName);
        }

        var result = await _api.PostAsync<object>("events", content);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", $"API Error: {result.ErrorMessage}");
            return View(vm);
        }

        TempData["Success"] = "Event created successfully.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var evResult = await _api.GetAsync<EventDto>($"events/{id}");
        if (!evResult.IsSuccess || evResult.Data == null) return NotFound();

        var catsResult = await _api.GetAsync<List<CategoryDto>>($"events/{id}/categories");

        return View(new EventFormViewModel
        {
            EventID = evResult.Data.EventID,
            EventName = evResult.Data.EventName,
            Description = evResult.Data.Description,
            EventDate = evResult.Data.EventDate,
            Location = evResult.Data.Location,
            Distance = evResult.Data.Distance,
            EventType = evResult.Data.EventType,
            Categories = catsResult.Data ?? new List<CategoryDto>()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EventFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.PutAsync<EventDto>($"events/{vm.EventID}", new
        {
            EventName = vm.EventName,
            Description = vm.Description,
            EventDate = vm.EventDate,
            Location = vm.Location,
            Distance = vm.Distance,
            EventType = vm.EventType
        });

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", $"API Error: {result.ErrorMessage}");
            return View(vm);
        }

        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _api.DeleteAsync($"events/{id}");
        TempData[ok ? "Success" : "Error"] = ok ? "Event deleted." : "Cannot delete (has enrolments).";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(int eventId, string categoryName, string? description)
    {
        var result = await _api.PostAsync<CategoryDto>($"events/{eventId}/categories", new
        {
            CategoryName = categoryName,
            Description = description
        });
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Category added." : "Failed.";
        return RedirectToAction(nameof(Edit), new { id = eventId });
    }

    public async Task<IActionResult> Enrollments(int eventId)
    {
        var evResult = await _api.GetAsync<EventDto>($"events/{eventId}");
        var enrolResult = await _api.GetAsync<List<EnrollmentDto>>($"events/{eventId}/enrollments");

        ViewBag.EventName = evResult.Data?.EventName ?? "Event";
        ViewBag.EventID = eventId;
        return View(enrolResult.Data ?? new List<EnrollmentDto>());
    }

    public async Task<IActionResult> Results(int eventId)
    {
        var evResult = await _api.GetAsync<EventDto>($"events/{eventId}");
        var enrolResult = await _api.GetAsync<List<EnrollmentDto>>($"events/{eventId}/enrollments");

        return View(new ResultEntryViewModel
        {
            EventID = eventId,
            EventName = evResult.Data?.EventName ?? "",
            Enrollments = enrolResult.Data ?? new List<EnrollmentDto>()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CaptureResult(int eventId, int enrollmentId, string finishTime, int position)
    {
        if (TimeSpan.TryParse(finishTime, out var ts))
        {
            var result = await _api.PostAsync<ResultDto>("results", new
            {
                EnrollmentId = enrollmentId,
                FinishTime = ts,
                Position = position
            });
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Result saved." : "Failed.";
        }
        return RedirectToAction(nameof(Results), new { eventId });
    }
}
