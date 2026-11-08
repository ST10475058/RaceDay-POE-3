using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class ApiControllers : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IConfiguration _config;

    public ApiControllers(RaceDayDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // --- AUTHENTICATION ---
    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        try
        {
            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                return BadRequest(new { message = "Email already exists." });

            var user = new User
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Registered successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKeyForRaceDayApplication2026!");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new
        {
            UserID = user.UserID,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Token = tokenHandler.WriteToken(token)
        });
    }

    [HttpPost("auth/logout")]
    public IActionResult Logout() => Ok();

    // --- EVENTS ---
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents() => Ok(await _db.Events.ToListAsync());

    [HttpGet("events/{id}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        return ev == null ? NotFound() : Ok(ev);
    }

    [HttpPost("events")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        [FromForm] string EventName,
        [FromForm] string? Description,
        [FromForm] string EventDate,
        [FromForm] string Location,
        [FromForm] string Distance,
        [FromForm] string EventType,
        IFormFile? BannerImage)
    {
        try
        {
            // Safe parsing of the form data
            DateTime parsedDate = DateTime.Parse(EventDate);
            decimal parsedDistance = decimal.Parse(Distance);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            string? bannerUrl = BannerImage != null ? "https://via.placeholder.com/600x300?text=Event+Banner" : null;

            var ev = new Event
            {
                EventName = EventName,
                Description = Description,
                EventDate = parsedDate,
                Location = Location,
                Distance = parsedDistance,
                EventType = EventType,
                OrganiserID = userId,
                BannerUrl = bannerUrl
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            return Ok(ev);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpPut("events/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updated)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        ev.EventName = updated.EventName;
        ev.Description = updated.Description;
        ev.EventDate = updated.EventDate;
        ev.Location = updated.Location;
        ev.Distance = updated.Distance;
        ev.EventType = updated.EventType;
        await _db.SaveChangesAsync();
        return Ok(ev);
    }

    [HttpDelete("events/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return Ok();
    }

    // --- CATEGORIES ---
    [HttpGet("events/{eventId}/categories")]
    public async Task<IActionResult> GetCategories(int eventId) => Ok(await _db.Categories.Where(c => c.EventID == eventId).ToListAsync());

    [HttpPost("events/{eventId}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> AddCategory(int eventId, [FromBody] Category cat)
    {
        cat.EventID = eventId;
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return Ok(cat);
    }

    // --- ENROLLMENTS ---
    [HttpGet("enrollments/my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> MyEnrollments()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(await _db.Enrollments.Where(e => e.UserID == userId).ToListAsync());
    }

    [HttpGet("events/{eventId}/enrollments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> EventEnrollments(int eventId)
    {
        return Ok(await _db.Enrollments.Where(e => e.EventID == eventId).ToListAsync());
    }

    [HttpPost("enrollments")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enroll([FromBody] Enrollment en)
    {
        en.UserID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _db.Enrollments.Add(en);
        await _db.SaveChangesAsync();
        return Ok(en);
    }

    // --- RESULTS ---
    [HttpGet("results/my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> MyResults()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var myEnrollments = await _db.Enrollments.Where(e => e.UserID == userId).Select(e => e.EnrollmentID).ToListAsync();
        return Ok(await _db.Results.Where(r => myEnrollments.Contains(r.EnrollmentID)).ToListAsync());
    }

    [HttpPost("results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> AddResult([FromBody] Result res)
    {
        _db.Results.Add(res);
        await _db.SaveChangesAsync();
        return Ok(res);
    }

    // --- PROFILE ---
    [HttpGet("users/{userId}/profile")]
    [Authorize]
    public IActionResult GetProfile(int userId) => Ok(new { DateOfBirth = "2000-01-01", Phone = "1234567890", City = "City" });

    [HttpPut("users/{userId}/profile")]
    [Authorize]
    public IActionResult UpdateProfile(int userId) => Ok();
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}