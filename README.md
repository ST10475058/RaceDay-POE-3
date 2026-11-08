# RaceDay — Part 3: MVC Web Application

RaceDay is a full-stack, web-based event management system for the South African road running, walking, and cycling community. Part 3 implements the MVC Web Frontend that consumes the RESTful API, built with ASP.NET Core 8.0 MVC, Razor Views, and Bootstrap. It provides a polished, role-specific interface for Organisers and Participants, and is fully containerised using Docker.

**Repository:** [https://github.com/ST10475058/RaceDay-POE-3.git](https://github.com/ST10475058/RaceDay-POE-3.git)

## Roles

- **Organiser** – creates, edits, and deletes events; manages categories; captures results; views all enrolments for their events. Includes file upload for event banners.
- **Participant** – browses events, enters events by selecting a category, views their own enrolments, and tracks personal results. Includes profile management.

## Setup and Run (Local)

1. Clone the repository: `git clone https://github.com/ST10475058/RaceDay-POE-3.git`
2. Open the solution file (`.sln`) in Visual Studio 2022.
3. Ensure the `RaceDay.API` and `RaceDay.Web` projects are both set as **Multiple Startup Projects**.
4. Open the Package Manager Console, select `RaceDay.API` as the default project, and run: `Update-Database`
5. Press **F5** to run. Both Swagger (API) and the MVC Web App will launch.

**Prerequisites:** .NET 8.0 SDK · SQL Server (LocalDB/Express) · Visual Studio 2022

## Docker Setup

The MVC application is fully containerised.

1. Build the Docker image:
   `docker build -t raceday-web -f RaceDay.Web/Dockerfile .`
2. Run the container:
   `docker run -d -p 8080:80 --name raceday-web-container raceday-web`
3. Access the application at: `http://localhost:8080`

## MVC Architecture & Features

- **No Direct Database Access:** The MVC application does not connect to a database. All data operations are handled via HTTP requests to the API using the `RaceDayApiClient` service.
- **Session Management:** Upon successful login, the JWT token and user role are stored securely in the session. The navigation bar dynamically updates based on the user's role.
- **File Handling:** Organiser banner uploads are sent to the API via `multipart/form-data`. The API handles the storage and returns the URL for the MVC frontend to display.
- **UI Design:** Energetic, sporting theme using Bootstrap 5 and custom CSS with colour-coded statuses (green for Confirmed).

## CI/CD

CI/CD Green Build

![Build and Test](part3-green-build.png)

[![Build and Test](https://github.com/ST10475058/RaceDay-POE-3/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/ST10475058/RaceDay-POE-3/actions/workflows/build-and-test.yml)

The GitHub Actions workflow builds the entire solution (API and MVC) on every push. All steps pass successfully.

## Video Walkthrough

[Watch the video walkthrough here](INSERT_YOUR_YOUTUBE_LINK_HERE)

## References

- Microsoft Learn. *ASP.NET Core MVC*. https://learn.microsoft.com/en-us/aspnet/core/mvc/ (accessed October 2026)
- Microsoft Learn. *HttpClient in .NET*. https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient (accessed October 2026)
- Microsoft Learn. *Docker for ASP.NET Core*. https://learn.microsoft.com/en-us/dotnet/core/docker/ (accessed October 2026)
- Bootstrap. *Bootstrap 5 Documentation*. https://getbootstrap.com/docs/5.3/ (accessed October 2026)
- Rosebank College. *RaceDay Portfolio of Evidence assignment brief*, 2026
- Anthropic. *Claude (AI assistant)* – Used for structuring and documentation. All design decisions reviewed by the author.
