# RaceDay
# RaceDay — Part 2: RESTful API Development

## Project Overview

RaceDay is a race event management system developed using ASP.NET Core Web API, C#, Entity Framework Core, and Microsoft SQL Server.

The API provides backend functionality for managing users, events, event categories, participant enrolments, and race results. It implements authentication and role-based authorization for Organisers and Participants.

This project forms Part 2 of the RaceDay system development and provides the API that will support the MVC frontend in Part 3.

## Technologies Used

- C#
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server
- Cookie-based authentication
- Password hashing
- OpenAPI / Swagger
- Unit testing
- Git and GitHub

## User Roles

### Organiser

Organisers manage race events and associated information within their authorized permissions. Their responsibilities include creating and managing events, defining event categories, viewing enrolments for their events, and recording participant results.

### Participant

Participants can register and log in, view available events and categories, enrol in events, manage their own profiles, and view their own enrolments and results.

Role-based authorization restricts access to protected endpoints.

## Main API Endpoints

The following table describes the intended API routes. Confirm the final routes against the implemented controllers.

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/auth/register` | Register an account |
| POST | `/api/auth/login` | Authenticate a user |
| POST | `/api/auth/logout` | Log out an authenticated user |
| GET | `/api/auth/me` | View the current user's profile |
| PUT | `/api/auth/me` | Update the current user's profile |
| GET | `/api/events` | Retrieve events |
| GET | `/api/events/{id}` | Retrieve an event by ID |
| POST | `/api/events` | Create an event |
| PUT | `/api/events/{id}` | Update an event |
| DELETE | `/api/events/{id}` | Delete an event |
| GET | `/api/categories` | Retrieve categories |
| GET | `/api/categories/{id}` | Retrieve a category |
| POST | `/api/categories` | Create a category |
| PUT | `/api/categories/{id}` | Update a category |
| DELETE | `/api/categories/{id}` | Delete a category |
| GET | `/api/enrollments` | Retrieve authorized enrolments |
| GET | `/api/enrollments/{id}` | Retrieve an enrolment |
| POST | `/api/enrollments` | Enrol a participant |
| DELETE | `/api/enrollments/{id}` | Delete an enrolment |
| GET | `/api/results` | Retrieve authorized results |
| GET | `/api/results/{id}` | Retrieve a result |
| POST | `/api/results` | Record a result |
| PUT | `/api/results/{id}` | Update a result |
| DELETE | `/api/results/{id}` | Delete a result |

## Database

The application uses Microsoft SQL Server with Entity Framework Core.

The existing database contains these tables:

- `User`
- `Event`
- `EventType`
- `Category`
- `Enrollment`
- `Result`

These tables store user information, event details, event categories, participant enrolments, and race results.

## Prerequisites

Before running the project, ensure that you have:

- Visual Studio with ASP.NET and web development support.
- The .NET SDK version targeted by the project.
- Microsoft SQL Server and SQL Server Management Studio.
- The RaceDay database and its required tables.
- The NuGet packages specified in the project file.

## Installation and Setup

### 1. Clone the repository

Replace the placeholder with your actual GitHub repository URL.

```bash
git clone YOUR_GITHUB_REPOSITORY_URL
```

### 2. Open the project

Open the RaceDay solution in Visual Studio and restore the NuGet packages.

### 3. Configure the database

Ensure that SQL Server is running and that the RaceDay database exists.

The local development connection string is:

```text
Server=localhost\SQLEXPRESS;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;
```

Update the connection string if your SQL Server instance differs. Do not commit production credentials or sensitive configuration to the repository.

### 4. Build the solution

In Visual Studio, select **Build → Build Solution**. Resolve any compilation errors before proceeding.

### 5. Run the API

Press **F5** in Visual Studio to start the application.

The local HTTP address previously used during development was:

```text
http://localhost:5294
```

The actual address may differ depending on the launch profile and port configuration.

### 6. Access API documentation

The project exposes an OpenAPI JSON document at:

```text
http://localhost:5294/openapi/v1.json
```

If interactive Swagger UI is configured, open its configured URL, commonly:

```text
http://localhost:5294/swagger
```

## Authentication and Security

The API uses password hashing rather than storing passwords in plain text. Authentication establishes a user's identity, and role-based authorization restricts access to protected functionality.

Public registration must not grant unauthorized Organiser privileges. Users should only be able to update their own profiles, Participants must not access other Participants' private records, and Organisers must only manage resources within their authorized scope.

Password hashes and other sensitive authentication information must never be returned in API responses.

## Testing

Run the unit tests using Visual Studio Test Explorer or the .NET CLI:

```bash
dotnet test
```

Tests should cover registration and login, password hashing, authentication and authorization, role restrictions, event management, participant enrolments, access to private records, invalid input, and missing records.

Test results should be reported only after the tests have actually been executed.

## Continuous Integration and Build Evidence

The assignment requires a GitHub Actions workflow that builds the solution and runs the unit tests on every push.

**Successful CI/CD build screenshot**

Add a screenshot of the actual successful GitHub Actions workflow run below. Ensure the image exists at the specified path.

![Successful GitHub Actions build](docs/github-actions-success.png)

## Demonstration Video

The assignment requires an unlisted YouTube video demonstrating the running API, project structure, endpoints, authentication, and unit tests.

**YouTube demonstration:** [Insert your unlisted YouTube video URL here]

## Project Structure

The project includes the following main components:

```text
RaceDay/
├── Controllers/
│   ├── AuthController.cs
│   ├── EventsController.cs
│   ├── CategoriesController.cs
│   ├── EnrollmentsController.cs
│   └── ResultsController.cs
├── Data/
│   └── RaceDayContext.cs
├── Models/
│   ├── User.cs
│   ├── Event.cs
│   ├── EventType.cs
│   ├── Category.cs
│   ├── Enrollment.cs
│   └── Result.cs
├── Properties/
├── Program.cs
├── appsettings.json
├── RaceDay.http
└── RaceDay.csproj
```

The actual solution may contain additional files, including test projects and configuration files.

## Author

**Student:** Lehlogonolo Molobela

**Project:** RaceDay — Part 2: RESTful API Development

**Academic year:** 2026
