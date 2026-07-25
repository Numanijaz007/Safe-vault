using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Services;
using System.Security.Claims;

namespace SafeVault.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // has to be logged in for any of this
    public class VaultItemsController : ControllerBase
    {
        private readonly SafeVaultDbContext db;

        public VaultItemsController(SafeVaultDbContext db)
        {
            this.db = db;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // regular users only see their own stuff, admins see everything
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IQueryable<VaultItem> query = db.VaultItems;

            if (!User.IsInRole("Admin"))
                query = query.Where(v => v.OwnerId == CurrentUserId);

            var items = await query.ToListAsync();

            // encode before sending back out just in case content has anything
            // weird in it from before validation was tightened up
            foreach (var item in items)
            {
                item.Title = InputValidator.SanitizeForOutput(item.Title);
            }

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            // using EF Core's parameterized query here (FirstOrDefaultAsync with
            // a lambda) instead of building a raw SQL string - this is what
            // actually protects against SQL injection, not just the regex check
            var item = await db.VaultItems.FirstOrDefaultAsync(v => v.Id == id);

            if (item == null)
                return NotFound();

            if (item.OwnerId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            item.Title = InputValidator.SanitizeForOutput(item.Title);
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VaultItem item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (InputValidator.ContainsSqlInjectionAttempt(item.Title) ||
                InputValidator.ContainsSqlInjectionAttempt(item.Content))
            {
                return BadRequest("input contains characters that aren't allowed");
            }

            item.OwnerId = CurrentUserId;
            db.VaultItems.Add(item);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VaultItem updated)
        {
            var item = await db.VaultItems.FirstOrDefaultAsync(v => v.Id == id);
            if (item == null)
                return NotFound();

            if (item.OwnerId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            if (InputValidator.ContainsSqlInjectionAttempt(updated.Title) ||
                InputValidator.ContainsSqlInjectionAttempt(updated.Content))
            {
                return BadRequest("input contains characters that aren't allowed");
            }

            item.Title = updated.Title;
            item.Content = updated.Content;
            await db.SaveChangesAsync();

            return NoContent();
        }

        // only admins can delete - regular users can't wipe stuff even if it's theirs
        // (figured better safe than sorry for a "vault" app)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await db.VaultItems.FirstOrDefaultAsync(v => v.Id == id);
            if (item == null)
                return NotFound();

            db.VaultItems.Remove(item);
            await db.SaveChangesAsync();

            return NoContent();
        }
    }
}
