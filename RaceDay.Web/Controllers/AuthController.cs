using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models.DTOs;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;
using System.Security.Claims;

namespace RaceDay.Web.Controllers;

public class AuthController : Controller
{
    private readonly RaceDayApiClient _api;
    public AuthController(RaceDayApiClient api) => _api = api;

    [HttpGet] public IActionResult Login() => View();
    [HttpGet] public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.PostAsync<AuthResponse>("auth/login", new
        {
            Email = vm.Email,
            Password = vm.Password
        });

        if (!result.IsSuccess || result.Data == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(vm);
        }

        var authData = result.Data;

        HttpContext.Session.SetString("JwtToken", authData.Token);
        HttpContext.Session.SetString("UserRole", authData.Role);
        HttpContext.Session.SetString("UserName", $"{authData.FirstName} {authData.LastName}");
        HttpContext.Session.SetInt32("UserId", authData.UserID);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, $"{authData.FirstName} {authData.LastName}"),
            new(ClaimTypes.Email, authData.Email),
            new(ClaimTypes.Role, authData.Role),
            new(ClaimTypes.NameIdentifier, authData.UserID.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return authData.Role == "Organiser"
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Participant");
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.PostAsync<object>("auth/register", new
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Email = vm.Email,
            Password = vm.Password,
            Role = vm.Role
        });

        if (!result.IsSuccess)
        {
            // THIS WILL NOW SHOW THE REAL ERROR FROM THE API
            ModelState.AddModelError("", $"Registration failed: {result.ErrorMessage}");
            return View(vm);
        }

        TempData["Success"] = "Registered successfully. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.PostAsync("auth/logout", new { });
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}