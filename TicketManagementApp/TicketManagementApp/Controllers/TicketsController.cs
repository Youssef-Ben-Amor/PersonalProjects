using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketManagementApp.Data;
using TicketManagementApp.Models;

namespace TicketManagementApp.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------------------- INDEX ----------------------
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Users");
                
            ViewBag.Admin = await _userManager.IsInRoleAsync(user, "Admin");
            ViewBag.User = user;

            var tickets = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .ToListAsync();

            return View(tickets);
        }

        // ---------------------- DETAILS ----------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        // ---------------------- CREATE GET ----------------------
        public IActionResult Create()
        {
            LoadDropDowns();
            return View();
        }

        // ---------------------- CREATE POST ----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Users");

            ticket.CreatedByUserId = user.Id;

            // Required navigation properties are removed
            ModelState.Remove("CreatedByUser");
            ModelState.Remove("AssignedToUser");
            ModelState.Remove("CreatedByUserId");

            if (string.IsNullOrWhiteSpace(ticket.AssignedToUserId))
                ticket.AssignedToUserId = null;

            if (ModelState.IsValid)
            {
                ticket.CreatedAt = DateTime.Now;

                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropDowns(ticket);
            return View(ticket);
        }

        // ---------------------- EDIT GET ----------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound();

            LoadDropDowns(ticket);
            return View(ticket);
        }

        // ---------------------- EDIT POST ----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ticket ticket)
        {
            if (id != ticket.Id)
                return NotFound();

            ModelState.Remove("CreatedByUser");
            ModelState.Remove("AssignedToUser");

            if (string.IsNullOrWhiteSpace(ticket.AssignedToUserId))
                ticket.AssignedToUserId = null;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            LoadDropDowns(ticket);
            return View(ticket);
        }

        // ---------------------- DELETE GET ----------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        // ---------------------- DELETE POST ----------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------------------- Helpers ----------------------
        private void LoadDropDowns(Ticket? ticket = null)
        {
            // Assigned to user
            ViewBag.AssignedToUserId = new SelectList(_context.Users, "Id", "FullName", ticket?.AssignedToUserId);

            // Status
            ViewBag.StatusList = new SelectList(new[]
            {
                "Open", "InProgress", "Resolved", "Closed"
            }, ticket?.Status);

            // Category
            ViewBag.CategoryList = new SelectList(new[]
            {
                "Bug", "Improvement", "Access", "Feature"
            }, ticket?.Category);

            // Urgency (1 to 5)
            ViewBag.UrgencyList = new SelectList(new[]
            {
                new { Value = 1, Text = "Low" },
                new { Value = 2, Text = "Medium" },
                new { Value = 3, Text = "High" },
                new { Value = 4, Text = "Urgent" },
                new { Value = 5, Text = "Critical" }
            }, "Value", "Text", ticket?.UrgencyLevel);
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
