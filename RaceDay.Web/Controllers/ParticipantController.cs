using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models.DTOs;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Participant")]
public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;
    public ParticipantController(RaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<EventDto>>("events");
        return View(result.Data ?? new List<EventDto>());
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _api.GetAsync<EventDto>($"events/{id}");
        if (!result.IsSuccess || result.Data == null) return NotFound();

        var catsResult = await _api.GetAsync<List<CategoryDto>>($"events/{id}/categories");
        ViewBag.Categories = catsResult.Data ?? new List<CategoryDto>();
        return View(result.Data);
    }

    public async Task<IActionResult> Enrol(int eventId)
    {
        var evResult = await _api.GetAsync<EventDto>($"events/{eventId}");
        if (!evResult.IsSuccess || evResult.Data == null) return NotFound();

        var catsResult = await _api.GetAsync<List<CategoryDto>>($"events/{eventId}/categories");

        return View(new EnrollViewModel
        {
            EventID = eventId,
            Event = evResult.Data,
            Categories = catsResult.Data ?? new List<CategoryDto>()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Enrol(EnrollViewModel vm)
    {
        var result = await _api.PostAsync("enrollments", new
        {
            EventId = vm.EventID,
            CategoryId = vm.CategoryID
        });

        if (!result.IsSuccess)
        {
            TempData["Error"] = "Could not enrol (already enrolled or category invalid).";
            return RedirectToAction(nameof(Details), new { id = vm.EventID });
        }

        TempData["Success"] = "Successfully enrolled!";
        return RedirectToAction(nameof(MyEvents));
    }

    public async Task<IActionResult> MyEvents()
    {
        var enrolResult = await _api.GetAsync<List<EnrollmentDto>>("enrollments/my");
        var events = new List<EventDto>();

        if (enrolResult.Data != null)
        {
            foreach (var e in enrolResult.Data)
            {
                var evResult = await _api.GetAsync<EventDto>($"events/{e.EventID}");
                if (evResult.Data != null) events.Add(evResult.Data);
            }
        }

        ViewBag.Enrolments = enrolResult.Data;
        return View(events);
    }

    public async Task<IActionResult> MyResults()
    {
        var result = await _api.GetAsync<List<ResultDto>>("results/my");
        return View(result.Data ?? new List<ResultDto>());
    }

    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var result = await _api.GetAsync<ProfileDto>($"users/{userId}/profile");
        return View(result.Data ?? new ProfileDto { UserID = userId.Value });
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileDto vm)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var result = await _api.PutAsync($"users/{userId}/profile", new
        {
            DateOfBirth = vm.DateOfBirth,
            Phone = vm.Phone,
            City = vm.City
        });

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Profile saved." : "Update failed.";
        return RedirectToAction(nameof(Profile));
    }
}