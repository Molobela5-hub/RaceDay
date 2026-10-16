
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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/events
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return Ok(await _context.Events
                .AsNoTracking()
                .ToListAsync());
        }

        // GET: api/events/5
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
                return NotFound();

            return Ok(raceEvent);
        }

        // POST: api/events
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event raceEvent)
        {
            // Ensure organiser cannot set OrganiserID for another user.
            raceEvent.OrganiserID = CurrentUserId;

            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent);
        }

        // PUT: api/events/5
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(
            int id, Event updatedEvent)
        {
            if (id != updatedEvent.EventID)
                return BadRequest("Event ID mismatch.");

            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
                return NotFound();

            // Only the organiser who owns the event may update it.
            if (existingEvent.OrganiserID != CurrentUserId)
                return Forbid();

            // Do not allow changing the organiser.
            updatedEvent.OrganiserID = existingEvent.OrganiserID;

            _context.Entry(existingEvent)
                .CurrentValues.SetValues(updatedEvent);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/events/5
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
                return NotFound();
            if (raceEvent.OrganiserID != CurrentUserId)
                return Forbid();

            _context.Events.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
