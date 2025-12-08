using System.ComponentModel.DataAnnotations;

namespace TicketManagementApp.Models.ViewModels
{
    public class ProfilViewModel
    {
        public string UserId { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        public List<Ticket> CreatedTickets { get; set; }
        public List<Ticket> AssignedTickets { get; set; }

    }
}
