using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return User.IsInRole("Organiser")
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Participant");
        }
        return View();
    }

    public IActionResult Error() => View();
}