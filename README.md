RaceDay Part3– Race Management System

1. Project Overview

RaceDay is a web-based race management system developed using ASP.NET Core Web API and ASP.NET Core MVC. The system allows organisers to manage racing events, categories, participant enrolments and race results.

Participants can register, log in, browse upcoming events, enrol in races, manage their enrolments and view their results.

The application uses a SQL Server database to store and manage system information.

2. Technologies Used

* C#
* ASP.NET Core Web API
* ASP.NET Core MVC
* .NET 10
* Entity Framework Core
* Microsoft SQL Server
* SQL Server Management Studio (SSMS)
* HTML
* CSS
* Bootstrap
* Docker
* Git and GitHub
* GitHub Actions

3. System Architecture

The RaceDay solution consists of two main projects:

RaceDay – Web API

The backend application responsible for:

* User authentication and authorisation.
* Event management.
* Category management.
* Participant enrolments.
* Race result management.
* Database communication.
* API endpoints.

RaceDay.MVC – MVC Frontend

The frontend application responsible for:

* Displaying the user interface.
* Participant registration and login.
* Organiser dashboard.
* Event browsing and management.
* Category selection.
* Enrolment management.
* Results display.

The MVC application communicates with the Web API through HTTP requests.

4. Main Features

Authentication and Authorisation

* Participant registration.
* User login and logout.
* Role-based access control.
* Separate Participant and Organiser functionality.
* Secure password hashing.
* Protected organiser functionality.

Organiser Features

* View organiser dashboard.
* Create, edit and manage events.
* Manage event categories.
* View participant enrolments.
* Manage race results.
* View event-related information.

Participant Features

* Register and log in.
* Browse upcoming events.
* View event details.
* Select event categories.
* Enrol in events.
* View personal enrolments.
* Edit eligible upcoming event enrolments.
* Cancel eligible enrolments.
* View race results.

Enrolment Management

* Participants can enrol in events.
* Duplicate enrolments are restricted.
* Participants can update eligible enrolments.
* Enrolments with recorded results cannot be edited.
* Category selection is validated against the relevant event.

5. Database

The application uses Microsoft SQL Server.

Database name: RaceDay

The database stores information relating to:

* Users
* Events
* Categories
* Enrolments
* Results
* Event types

Entity Framework Core is used for database operations and relationships.

6. Project Structure

RaceDay/
│
├── RaceDay/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── Dockerfile
│   └── Program.cs
│
├── RaceDay.MVC/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   ├── wwwroot/
│   ├── Dockerfile
│   └── Program.cs
│
├── docker-compose.yml
├── .dockerignore
├── .github/
└── RaceDay.slnx

7. Installation and Setup

Requirements

Install the following software:

* Visual Studio 2022 or compatible version.
* .NET 10 SDK.
* Microsoft SQL Server.
* SQL Server Management Studio (SSMS).

Docker Desktop is required if you want to run the application using Docker.

Step 1: Clone the Repository

git clone YOUR_GITHUB_REPOSITORY_URL

Open the cloned project folder.

Step 2: Open the Solution

Open:

RaceDay.slnx

using Visual Studio.

Step 3: Configure the Database

Make sure SQL Server is running.

Create or configure the RaceDay database and update the connection string in the API project’s configuration.

Example:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}

Use the database setup and migrations included in the project.

Step 4: Run the API

Set RaceDay as the startup project and run it.

The API is configured to run at:

http://localhost:5294

Step 5: Run the MVC Application

Set RaceDay.MVC as the startup project and run it separately.

The MVC application is available at:

https://localhost:7250

Both applications must be running for the complete system to function.

8. Docker Configuration

The project includes Dockerfiles for the API and MVC applications, together with a Docker Compose configuration.

The Compose configuration defines:

* raceday-api
* raceday-mvc

To build and start the containers:

docker compose up --build

To stop the containers:

docker compose down

The Docker configuration uses environment variables for database connection details and other configuration settings.

A suitable database connection string must be supplied for the containerised API to communicate with SQL Server.

9. Security

The application implements:

* Password hashing.
* Authentication and authorisation.
* Role-based access restrictions.
* Participant-specific enrolment access.
* Organiser-only management functionality.
* Validation of enrolment category selection.
* Restrictions on editing enrolments after results have been recorded.

Sensitive configuration values should be supplied through environment variables or appropriate configuration settings and should not be committed to GitHub.

10. Testing

The following application functionality should be verified:

* Participant registration and login.
* Organiser authentication.
* Event creation and editing.
* Category creation and management.
* Participant event enrolment.
* Duplicate enrolment prevention.
* Enrolment editing.
* Enrolment cancellation.
* Results creation and management.
* Participant results viewing.
* Event deletion restrictions.
* API and MVC communication.

11. Limitations and Additional Configuration

Azure Blob Storage integration has been included in the application code, but requires Azure Blob Storage configuration and valid credentials before image uploads can be used.

Docker configuration is included in the repository. Successful container execution requires the correct database connection configuration.

12. Demonstration Video

YouTube Demonstration:

[INSERT YOUR YOUTUBE VIDEO LINK HERE]

13. Developer

Project: RaceDay – Race Management System

Developer: Lehlogonolo Molobela

Academic Project: Part 3 – Portfolio of Evidence (POE)

Year: 2026

14. Conclusion

RaceDay provides a centralised platform for managing racing events, participants, categories, enrolments and results.

The project demonstrates the development of a web-based application using ASP.NET Core Web API, ASP.NET Core MVC, Entity Framework Core and Microsoft SQL Server, with additional Docker configuration for containerisation.