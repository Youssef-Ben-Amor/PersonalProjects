using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketManagementApp.Data;
using TicketManagementApp.Models;
using TicketManagementApp.Models.ViewModels;

namespace TicketManagementApp.Services
{
    public interface ITicketService
    {
        // Ticket Operations
        Task<List<Ticket>> GetAllTicketsAsync();
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<Ticket?> GetTicketWithDetailsAsync(int id);
        Task<bool> CreateTicketAsync(Ticket ticket, string createdByUserId);
        Task<bool> UpdateTicketAsync(Ticket ticket);
        Task<bool> DeleteTicketAsync(int id);
        Task<bool> TicketExistsAsync(int id);

        // User-specific Ticket Operations
        Task<List<Ticket>> GetTicketsCreatedByUserAsync(string userId);
        Task<List<Ticket>> GetTicketsAssignedToUserAsync(string userId);

        // User Operations
        Task<ApplicationUser?> GetUserAsync(System.Security.Claims.ClaimsPrincipal principal);
        Task<bool> IsUserInRoleAsync(ApplicationUser user, string role);
        Task<ProfilViewModel?> GetUserProfileAsync(string userId);

        // UI Helpers
        SelectList GetAssignedToUsersSelectList(string? selectedUserId = null);
        SelectList GetStatusSelectList(string? selectedStatus = null);
        SelectList GetCategorySelectList(string? selectedCategory = null);
        SelectList GetUrgencySelectList(int? selectedUrgency = null);
    }

    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== Ticket Operations ====================

        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets.FindAsync(id);
        }

        public async Task<Ticket?> GetTicketWithDetailsAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> CreateTicketAsync(Ticket ticket, string createdByUserId)
        {
            try
            {
                ticket.CreatedByUserId = createdByUserId;
                ticket.CreatedAt = DateTime.Now;

                // Normaliser l'AssignedToUserId si vide
                if (string.IsNullOrWhiteSpace(ticket.AssignedToUserId))
                {
                    ticket.AssignedToUserId = null;
                }

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                // Normaliser l'AssignedToUserId si vide
                if (string.IsNullOrWhiteSpace(ticket.AssignedToUserId))
                {
                    ticket.AssignedToUserId = null;
                }

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TicketExistsAsync(ticket.Id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteTicketAsync(int id)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return false;
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TicketExistsAsync(int id)
        {
            return await _context.Tickets.AnyAsync(e => e.Id == id);
        }

        // ==================== User-specific Ticket Operations ====================

        public async Task<List<Ticket>> GetTicketsCreatedByUserAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Where(t => t.CreatedByUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Ticket>> GetTicketsAssignedToUserAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Where(t => t.AssignedToUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        // ==================== User Operations ====================

        public async Task<ApplicationUser?> GetUserAsync(System.Security.Claims.ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }

        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<ProfilViewModel?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var createdTickets = await GetTicketsCreatedByUserAsync(userId);
            var assignedTickets = await GetTicketsAssignedToUserAsync(userId);

            return new ProfilViewModel
            {
                UserId = userId,
                FullName = user.FullName,
                Email = user.Email,
                CreatedTickets = createdTickets,
                AssignedTickets = assignedTickets
            };
        }

        // ==================== UI Helpers ====================

        public SelectList GetAssignedToUsersSelectList(string? selectedUserId = null)
        {
            var users = _context.Users
                .OrderBy(u => u.FullName)
                .AsNoTracking()
                .ToList();

            return new SelectList(users, "Id", "FullName", selectedUserId);
        }

        public SelectList GetStatusSelectList(string? selectedStatus = null)
        {
            var statuses = new[]
            {
                "Open",
                "InProgress",
                "Resolved",
                "Closed"
            };

            return new SelectList(statuses, selectedStatus);
        }

        public SelectList GetCategorySelectList(string? selectedCategory = null)
        {
            var categories = new[]
            {
                "Bug",
                "Improvement",
                "Access",
                "Feature"
            };

            return new SelectList(categories, selectedCategory);
        }

        public SelectList GetUrgencySelectList(int? selectedUrgency = null)
        {
            var urgencyLevels = new[]
            {
                new { Value = 1, Text = "Low" },
                new { Value = 2, Text = "Medium" },
                new { Value = 3, Text = "High" },
                new { Value = 4, Text = "Urgent" },
                new { Value = 5, Text = "Critical" }
            };

            return new SelectList(urgencyLevels, "Value", "Text", selectedUrgency);
        }
    }
}