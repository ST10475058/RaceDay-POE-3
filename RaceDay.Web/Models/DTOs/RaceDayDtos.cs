namespace RaceDay.Web.Models.DTOs;

public class AuthResponse
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string Token { get; set; } = "";
}

public class EventDto
{
    public int EventID { get; set; }
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal Distance { get; set; }
    public string EventType { get; set; } = "";
    public int OrganiserID { get; set; }
    public string? BannerUrl { get; set; }
}

public class CategoryDto
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
}

public class EnrollmentDto
{
    public int EnrollmentID { get; set; }
    public int UserID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = "";
    public string? ParticipantName { get; set; }
}

public class ResultDto
{
    public int ResultID { get; set; }
    public int EnrollmentID { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
}

public class ProfileDto
{
    public int ProfileID { get; set; }
    public int UserID { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
}