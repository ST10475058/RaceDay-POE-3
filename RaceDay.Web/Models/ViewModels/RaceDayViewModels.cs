using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.Models.DTOs;

namespace RaceDay.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public class RegisterViewModel
{
    [Required]
    public string FirstName { get; set; } = "";

    [Required]
    public string LastName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "Participant";
}

public class EventFormViewModel
{
    public int EventID { get; set; }

    [Required]
    public string EventName { get; set; } = "";

    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

    [Required]
    public string Location { get; set; } = "";

    [Required, Range(0.01, 999.99)]
    public decimal Distance { get; set; }

    [Required]
    public string EventType { get; set; } = "Run";

    public IFormFile? BannerImage { get; set; }
    public string? ExistingBannerUrl { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

public class EnrollViewModel
{
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
    public EventDto Event { get; set; } = null!;
}

public class ResultEntryViewModel
{
    public int EventID { get; set; }
    public string EventName { get; set; } = "";
    public List<EnrollmentDto> Enrollments { get; set; } = new();
    public Dictionary<int, ResultDto> Results { get; set; } = new();
}

public class ProfileViewModel
{
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
}