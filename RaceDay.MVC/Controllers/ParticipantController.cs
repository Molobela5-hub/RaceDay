using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using RaceDay.MVC.Models.Api;
using RaceDay.MVC.Models.ViewModels.Participant;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[Authorize(Roles = "Participant")]
public class ParticipantController : Controller
{
    private readonly IEventsApiService _eventsApi;
    private readonly ICategoriesApiService _categoriesApi;
    private readonly IEnrollmentsApiService _enrollmentsApi;
    private readonly IResultsApiService _resultsApi;
    private readonly IAuthApiService _authApi;

    public ParticipantController(
        IEventsApiService eventsApi,
        ICategoriesApiService categoriesApi,
        IEnrollmentsApiService enrollmentsApi,
        IResultsApiService resultsApi,
        IAuthApiService authApi)
    {
        _eventsApi = eventsApi;
        _categoriesApi = categoriesApi;
        _enrollmentsApi = enrollmentsApi;
        _resultsApi = resultsApi;
        _authApi = authApi;
    }

    public async Task<IActionResult> UpcomingEvents()
    {
        var eventsResponse = await _eventsApi.GetAllAsync();
        if (!eventsResponse.Success)
        {
            TempData["Error"] = eventsResponse.ErrorMessage;
            return View(new List<EventDto>());
        }

        var today = DateTime.Today;

        var events = eventsResponse.Data?
            .Where(x => x.EventDate.Date >= today)
            .OrderBy(x => x.EventDate)
            .ToList() ?? [];

        return View(events);
    }

    public async Task<IActionResult> EventDetails(int id)
    {
        var eventResponse = await _eventsApi.GetByIdAsync(id);
        if (!eventResponse.Success || eventResponse.Data is null)
        {
            TempData["Error"] = eventResponse.ErrorMessage ?? "Event not found.";
            return RedirectToAction(nameof(UpcomingEvents));
        }

        var categoriesResponse = await _categoriesApi.GetAllAsync();
        var categories = categoriesResponse.Data?
            .Where(c => c.EventID == id)
            .OrderBy(c => c.CategoryName)
            .ToList() ?? [];

        return View(new ParticipantEventDetailsViewModel
        {
            Event = eventResponse.Data,
            Categories = categories
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var create = await _enrollmentsApi.CreateAsync(new EnrollmentDto
        {
            EventID = eventId,
            CategoryID = categoryId
        });

        if (!create.Success)
        {
            TempData["Error"] = create.StatusCode == HttpStatusCode.Conflict
                ? "You are already enrolled in this event."
                : create.ErrorMessage ?? "Enrolment failed.";
            return RedirectToAction(nameof(EventDetails), new { id = eventId });
        }

        TempData["Success"] = "Enrolment successful.";
        return RedirectToAction(nameof(MyEnrolments));
    }

    public async Task<IActionResult> MyEnrolments()
    {
        var enrollmentsResponse = await _enrollmentsApi.GetAllAsync();
        var eventsResponse = await _eventsApi.GetAllAsync();
        var categoriesResponse = await _categoriesApi.GetAllAsync();
        var resultsResponse = await _resultsApi.GetAllAsync();

        if (!enrollmentsResponse.Success)
        {
            TempData["Error"] = enrollmentsResponse.ErrorMessage;
            return View(new List<ParticipantEnrollmentRowViewModel>());
        }

        var eventsById = eventsResponse.Data?.ToDictionary(x => x.EventID) ?? new Dictionary<int, EventDto>();
        var categoriesByEventAndId = categoriesResponse.Data?
            .ToDictionary(x => (x.EventID, x.CategoryID))
            ?? new Dictionary<(int EventID, int CategoryID), CategoryDto>();
        var enrollmentIdsWithResult = resultsResponse.Data?
            .Select(x => x.EnrollmentID)
            .ToHashSet() ?? [];
        var now = DateTime.UtcNow;

        var rows = enrollmentsResponse.Data?
            .OrderByDescending(x => x.EnrollmentDate)
            .Select(x =>
            {
                eventsById.TryGetValue(x.EventID, out var ev);
                categoriesByEventAndId.TryGetValue((x.EventID, x.CategoryID), out var cat);

                return new ParticipantEnrollmentRowViewModel
                {
                    EnrollmentID = x.EnrollmentID,
                    EventID = x.EventID,
                    EventName = ev?.EventName ?? $"Event #{x.EventID}",
                    EventDate = ev?.EventDate ?? DateTime.MinValue,
                    Location = ev?.Location ?? "Unknown",
                    CategoryName = cat?.CategoryName ?? $"Category #{x.CategoryID}",
                    EnrollmentDate = x.EnrollmentDate,
                    CanEdit = ev is not null && ev.EventDate > now && !enrollmentIdsWithResult.Contains(x.EnrollmentID)
                };
            }).ToList() ?? [];

        return View(rows);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var result = await _enrollmentsApi.DeleteAsync(id);

        if (!result.Success)
        {
            TempData["Error"] = result.StatusCode == HttpStatusCode.Conflict
                ? "You cannot cancel this enrolment because a race result has already been recorded."
                : result.ErrorMessage ?? "Cancellation failed.";

            return RedirectToAction(nameof(MyEnrolments));
        }

        TempData["Success"] = "Enrolment cancelled.";

        return RedirectToAction(nameof(MyEnrolments));
    }

    public async Task<IActionResult> EditEnrolment(int id)
    {
        var model = await LoadEditEnrollmentModel(id);
        if (model is null)
        {
            return RedirectToAction(nameof(MyEnrolments));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEnrolment(ParticipantEditEnrollmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var reload = await LoadEditEnrollmentModel(model.EnrollmentID, model.CategoryID);
            if (reload is null)
            {
                return RedirectToAction(nameof(MyEnrolments));
            }

            return View(reload);
        }

        var categoriesResponse = await _categoriesApi.GetAllAsync();
        var categoryBelongsToEvent = categoriesResponse.Data?
            .Any(x => x.EventID == model.EventID && x.CategoryID == model.CategoryID) == true;

        if (!categoryBelongsToEvent)
        {
            ModelState.AddModelError(nameof(model.CategoryID), "Selected category is not valid for this event.");
            var reload = await LoadEditEnrollmentModel(model.EnrollmentID, model.CategoryID);
            if (reload is null)
            {
                return RedirectToAction(nameof(MyEnrolments));
            }

            return View(reload);
        }

        var update = await _enrollmentsApi.UpdateAsync(new EnrollmentDto
        {
            EnrollmentID = model.EnrollmentID,
            EventID = model.EventID,
            CategoryID = model.CategoryID
        });

        if (!update.Success)
        {
            var error = update.ErrorMessage ?? "Could not update enrolment.";
            TempData["Error"] = error;
            ModelState.AddModelError(string.Empty, error);
            var reload = await LoadEditEnrollmentModel(model.EnrollmentID, model.CategoryID);
            if (reload is null)
            {
                return RedirectToAction(nameof(MyEnrolments));
            }

            return View(reload);
        }

        TempData["Success"] = "Enrolment updated.";
        return RedirectToAction(nameof(MyEnrolments));
    }

    public async Task<IActionResult> Results()
    {
        var resultsResponse = await _resultsApi.GetAllAsync();
        var enrollmentsResponse = await _enrollmentsApi.GetAllAsync();
        var eventsResponse = await _eventsApi.GetAllAsync();
        var categoriesResponse = await _categoriesApi.GetAllAsync();

        if (!resultsResponse.Success)
        {
            TempData["Error"] = resultsResponse.ErrorMessage;
            return View(new List<ParticipantResultRowViewModel>());
        }

        var enrollmentsById = enrollmentsResponse.Data?.ToDictionary(x => x.EnrollmentID) ?? new Dictionary<int, EnrollmentDto>();
        var eventsById = eventsResponse.Data?.ToDictionary(x => x.EventID) ?? new Dictionary<int, EventDto>();
        var categoriesByEventAndId = categoriesResponse.Data?
            .ToDictionary(x => (x.EventID, x.CategoryID))
            ?? new Dictionary<(int EventID, int CategoryID), CategoryDto>();

        var rows = resultsResponse.Data?
            .Select(x =>
            {
                enrollmentsById.TryGetValue(x.EnrollmentID, out var enrollment);
                eventsById.TryGetValue(enrollment?.EventID ?? 0, out var ev);
                categoriesByEventAndId.TryGetValue((enrollment?.EventID ?? 0, enrollment?.CategoryID ?? 0), out var cat);

                return new ParticipantResultRowViewModel
                {
                    ResultID = x.ResultID,
                    EventName = ev?.EventName ?? "Unknown",
                    EventDate = ev?.EventDate ?? DateTime.MinValue,
                    CategoryName = cat?.CategoryName ?? "Unknown",
                    FinishTime = x.FinishTime,
                    FinishingPosition = x.FinishingPosition
                };
            })
            .OrderBy(x => x.EventDate)
            .ToList() ?? [];

        return View(rows);
    }

    public async Task<IActionResult> Profile()
    {
        var me = await _authApi.MeAsync();
        if (!me.Success || me.Data is null)
        {
            TempData["Error"] = me.ErrorMessage ?? "Failed to load profile.";
            return View(new ProfileViewModel());
        }

        return View(new ProfileViewModel
        {
            FirstName = me.Data.FirstName,
            LastName = me.Data.LastName,
            Email = me.Data.Email,
            Phone = me.Data.Phone,
            Role = me.Data.Role
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var update = await _authApi.UpdateMeAsync(new UpdateProfileRequestDto
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone
        });

        if (!update.Success)
        {
            ModelState.AddModelError(string.Empty, update.ErrorMessage ?? "Profile update failed.");
            return View(model);
        }

        TempData["Success"] = "Profile updated.";
        model.Email = update.Data?.Email ?? model.Email;
        model.Role = update.Data?.Role ?? model.Role;

        return View(model);
    }

    private async Task<ParticipantEditEnrollmentViewModel?> LoadEditEnrollmentModel(
        int enrollmentId,
        int? selectedCategoryId = null)
    {
        var enrollmentResponse = await _enrollmentsApi.GetByIdAsync(enrollmentId);
        if (!enrollmentResponse.Success || enrollmentResponse.Data is null)
        {
            TempData["Error"] = enrollmentResponse.ErrorMessage ?? "Enrolment not found.";
            return null;
        }

        var enrollment = enrollmentResponse.Data;

        var eventResponse = await _eventsApi.GetByIdAsync(enrollment.EventID);
        if (!eventResponse.Success || eventResponse.Data is null)
        {
            TempData["Error"] = eventResponse.ErrorMessage ?? "Event not found.";
            return null;
        }

        if (eventResponse.Data.EventDate <= DateTime.UtcNow)
        {
            TempData["Error"] = "You can only edit enrolments for upcoming events.";
            return null;
        }

        var resultsResponse = await _resultsApi.GetAllAsync();
        if (resultsResponse.Data?.Any(x => x.EnrollmentID == enrollmentId) == true)
        {
            TempData["Error"] = "You cannot edit this enrolment because a race result has already been recorded.";
            return null;
        }

        var categoriesResponse = await _categoriesApi.GetAllAsync();
        var categories = categoriesResponse.Data?
            .Where(x => x.EventID == enrollment.EventID)
            .OrderBy(x => x.CategoryName)
            .ToList() ?? [];

        if (!categoriesResponse.Success)
        {
            TempData["Error"] = categoriesResponse.ErrorMessage ?? "Failed to load categories.";
        }

        var effectiveCategoryId = selectedCategoryId ?? enrollment.CategoryID;

        return new ParticipantEditEnrollmentViewModel
        {
            EnrollmentID = enrollment.EnrollmentID,
            EventID = enrollment.EventID,
            EventName = eventResponse.Data.EventName,
            EventDate = eventResponse.Data.EventDate,
            Location = eventResponse.Data.Location,
            CategoryID = effectiveCategoryId,
            CategoryOptions = categories
                .Select(x => new SelectListItem($"{x.CategoryName} ({x.CategoryType})", x.CategoryID.ToString()))
                .ToList()
        };
    }
}
