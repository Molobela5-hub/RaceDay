
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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);

        // GET: api/categories
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return Ok(await _context.Categories
                .AsNoTracking()
                .ToListAsync());
        }

        // GET: api/categories/5
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // POST: api/categories
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            // Verify the referenced event exists and belongs to the organiser.
            var ev = await _context.Events.FindAsync(category.EventID);

            if (ev == null)
                return BadRequest(new { message = "Event does not exist." });

            if (ev.OrganiserID != CurrentUserId)
                return Forbid();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                category);
        }

        // PUT: api/categories/5
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(
            int id, Category updatedCategory)
        {
            if (id != updatedCategory.CategoryID)
                return BadRequest("Category ID mismatch.");

            var existingCategory = await _context.Categories.FindAsync(id);

            if (existingCategory == null)
                return NotFound();

            // Only organisers owning the event may update the category.
            var ev = await _context.Events.FindAsync(existingCategory.EventID);

            if (ev == null)
                return BadRequest(new { message = "Referenced event does not exist." });

            if (ev.OrganiserID != CurrentUserId)
                return Forbid();

            // Prevent changing EventID to an event not owned by this organiser.
            if (updatedCategory.EventID != existingCategory.EventID)
            {
                var newEv = await _context.Events.FindAsync(updatedCategory.EventID);
                if (newEv == null)
                    return BadRequest(new { message = "New referenced event does not exist." });

                if (newEv.OrganiserID != CurrentUserId)
                    return Forbid();
            }

            _context.Entry(existingCategory)
                .CurrentValues.SetValues(updatedCategory);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categories/5
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            var ev = await _context.Events.FindAsync(category.EventID);

            if (ev == null)
                return BadRequest(new { message = "Referenced event does not exist." });

            if (ev.OrganiserID != CurrentUserId)
                return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
