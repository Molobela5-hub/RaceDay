
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrollmentsController(RaceDayContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: api/enrollments
        // Organisers can view enrolments; participants only their own.
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
        {
            var query = _context.Enrollments.AsNoTracking();

            if (User.IsInRole("Participant"))
            {
                query = query.Where(e => e.ParticipantID == CurrentUserId);
            }
            else if (User.IsInRole("Organiser"))
            {
                // Organisers may only view enrolments for their own events.
                query = query.Join(
                    _context.Events,
                    enrollment => enrollment.EventID,
                    ev => ev.EventID,
                    (enrollment, ev) => new { enrollment, ev })
                    .Where(x => x.ev.OrganiserID == CurrentUserId)
                    .Select(x => x.enrollment);
            }
            else
            {
                return Forbid();
            }

            return Ok(await query.ToListAsync());
        }

        // GET: api/enrollments/5
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Enrollment>> GetEnrollment(int id)
        {
            var enrollment = await _context.Enrollments
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EnrollmentID == id);

            if (enrollment == null)
                return NotFound();

            if (User.IsInRole("Participant") &&
                enrollment.ParticipantID != CurrentUserId)
                return Forbid();

            if (User.IsInRole("Organiser"))
            {
                var ev = await _context.Events.FindAsync(enrollment.EventID);
                if (ev == null)
                    return BadRequest(new { message = "Referenced event does not exist." });

                if (ev.OrganiserID != CurrentUserId)
                    return Forbid();
            }
            else if (!User.IsInRole("Participant"))
            {
                return Forbid();
            }

            return Ok(enrollment);
        }

        // POST: api/enrollments
        // Participants may enrol only themselves.
        [Authorize(Roles = "Participant")]
        [HttpPost]
        public async Task<ActionResult<Enrollment>> CreateEnrollment(
            Enrollment enrollment)
        {
            enrollment.ParticipantID = CurrentUserId;
            enrollment.EnrollmentDate = DateTime.UtcNow;

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventID == enrollment.EventID);

            if (!eventExists)
                return BadRequest(new { message = "Event does not exist." });

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == enrollment.CategoryID &&
                               c.EventID == enrollment.EventID);

            if (!categoryExists)
                return BadRequest(new
                {
                    message = "Category does not belong to the selected event."
                });

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrollment),
                new { id = enrollment.EnrollmentID },
                enrollment);
        }

        // DELETE: api/enrollments/5
        // Participants can cancel only their own enrolments.
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

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

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
