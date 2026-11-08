using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class User
{
    [Key] public int UserID { get; set; }
    [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string Role { get; set; } = string.Empty;
}

public class Event
{
    [Key] public int EventID { get; set; }
    [Required] public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public DateTime EventDate { get; set; }
    [Required] public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int OrganiserID { get; set; }
    public string? BannerUrl { get; set; }
}

public class Category
{
    [Key] public int CategoryID { get; set; }
    public int EventID { get; set; }
    [Required] public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class Enrollment
{
    [Key] public int EnrollmentID { get; set; }
    public int UserID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed";
}

public class Result
{
    [Key] public int ResultID { get; set; }
    public int EnrollmentID { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
}