
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: api/results
        // Participants see only their own results.
        // Organisers can view results.
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            var query = _context.Results
                .AsNoTracking()
                .Join(
                    _context.Enrollments,
                    result => result.EnrollmentID,
                    enrollment => enrollment.EnrollmentID,
                    (result, enrollment) => new { result, enrollment });

            if (User.IsInRole("Participant"))
            {
                query = query.Where(x =>
                    x.enrollment.ParticipantID == CurrentUserId);
            }
            else if (!User.IsInRole("Organiser"))
            {
                return Forbid();
            }

            return Ok(await query.Select(x => x.result).ToListAsync());
        }

        // GET: api/results/5
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            var result = await _context.Results
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
                return NotFound();

            var enrollment = await _context.Enrollments
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentID == result.EnrollmentID);

            if (enrollment == null)
                return NotFound();

            if (User.IsInRole("Participant"))
            {
                if (enrollment.ParticipantID != CurrentUserId)
                    return Forbid();
            }
            else if (User.IsInRole("Organiser"))
            {
                var ev = await _context.Events.FindAsync(enrollment.EventID);
                if (ev == null)
                    return BadRequest(new { message = "Referenced event does not exist." });

                if (ev.OrganiserID != CurrentUserId)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }

            return Ok(result);
        }

        // POST: api/results
        // Organisers record results.
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<Result>> CreateResult(Result result)
        {
            var enrollmentExists = await _context.Enrollments
                .AnyAsync(e => e.EnrollmentID == result.EnrollmentID);

            if (!enrollmentExists)
            {
                return BadRequest(new
                {
                    message = "The specified enrolment does not exist."
                });
            }

            // Ensure the organiser owns the event for this enrolment.
            var enrollment = await _context.Enrollments.FindAsync(result.EnrollmentID);
            if (enrollment == null)
                return BadRequest(new { message = "Referenced enrolment does not exist." });
            var ev = await _context.Events.FindAsync(enrollment.EventID);
            if (ev == null)
                return BadRequest(new { message = "Referenced event does not exist." });

            if (ev.OrganiserID != CurrentUserId)
                return Forbid();

            if (result.FinishingPosition < 1)
            {
                return BadRequest(new
                {
                    message = "Finishing position must be at least 1."
                });
            }

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                result);
        }

        // PUT: api/results/5
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateResult(
            int id, Result updatedResult)
        {
            if (id != updatedResult.ResultID)
                return BadRequest("Result ID mismatch.");

            var existingResult = await _context.Results.FindAsync(id);

            if (existingResult == null)
                return NotFound();

            var enrollmentExists = await _context.Enrollments
                .AnyAsync(e => e.EnrollmentID == updatedResult.EnrollmentID);

            if (!enrollmentExists)
            {
                return BadRequest(new
                {
                    message = "The specified enrolment does not exist."
                });
            }

            // Ensure organiser owns the related event for this enrollment.
            var enrollment = await _context.Enrollments.FindAsync(updatedResult.EnrollmentID);
            if (enrollment == null)
                return BadRequest(new { message = "Referenced enrolment does not exist." });
            var ev = await _context.Events.FindAsync(enrollment.EventID);
            if (ev == null)
                return BadRequest(new { message = "Referenced event does not exist." });

            if (ev.OrganiserID != CurrentUserId)
                return Forbid();

            if (updatedResult.FinishingPosition < 1)
            {
                return BadRequest(new
                {
                    message = "Finishing position must be at least 1."
                });
            }

            existingResult.EnrollmentID = updatedResult.EnrollmentID;
            existingResult.FinishTime = updatedResult.FinishTime;
            existingResult.FinishingPosition = updatedResult.FinishingPosition;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/results/5
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
                return NotFound();
            var enrollment = await _context.Enrollments.FindAsync(result.EnrollmentID);
            if (enrollment == null)
                return BadRequest(new { message = "Referenced enrolment does not exist." });

            var ev = await _context.Events.FindAsync(enrollment.EventID);
            if (ev == null)
                return BadRequest(new { message = "Referenced event does not exist." });

            if (ev.OrganiserID != CurrentUserId)
                return Forbid();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
