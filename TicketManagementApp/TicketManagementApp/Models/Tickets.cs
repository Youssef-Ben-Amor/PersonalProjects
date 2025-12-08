using System.ComponentModel.DataAnnotations;

namespace TicketManagementApp.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Ticket Title")]
        public string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        // ----- STATUS -----
        // Open, InProgress, Resolved, Closed
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Open|InProgress|Resolved|Closed)$",
            ErrorMessage = "Status must be Open, InProgress, Resolved, or Closed")]
        public string Status { get; set; } = "Open";

        // ----- URGENCY LEVEL -----
        // 1 = Low, 2 = Medium, 3 = High, 4 = Urgent, 5 = Critical
        [Required(ErrorMessage = "Urgency level is required")]
        [Range(1, 5, ErrorMessage = "Urgency must be between 1 and 5")]
        [Display(Name = "Urgency Level")]
        public int UrgencyLevel { get; set; } = 1;

        // ----- CATEGORY -----
        [Required(ErrorMessage = "Category is required")]
        [RegularExpression("^(Bug|Improvement|Access|Feature)$",
            ErrorMessage = "Category must be one of: Bug, Improvement, Access, Feature")]
        public string Category { get; set; }

        // ----- RELATIONS -----
        [Required(ErrorMessage = "Creator is required")]
        [Display(Name = "Created By")]
        public string CreatedByUserId { get; set; }
        public ApplicationUser CreatedByUser { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ----- HELPERS -----

        public string GetUrgencyLevelName()
        {
            return UrgencyLevel switch
            {
                1 => "Low ",
                2 => "Medium ",
                3 => "High ",
                4 => "Urgent",
                5 => "Critical",
                _ => "Unknown"
            };
        }

        public string GetUrgencyColor()
        {
            return UrgencyLevel switch
            {
                1 => "#22c55e", // Green
                2 => "#eab308", // Yellow
                3 => "#f97316", // Orange
                4 => "#ef4444", // Light Red
                5 => "#dc2626", // Dark Red
                _ => "#64748b" // Default gray
            };
        }
        public string GetUrgencyClass()
        {
            return UrgencyLevel switch
            {
                1 => "urgency-low",
                2 => "urgency-medium",
                3 => "urgency-high",
                4 => "urgency-critical",
                5 => "urgency-critical",
                _ => "" // Default gray
            };
        }
        
    }
}
