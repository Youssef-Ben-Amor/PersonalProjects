using Microsoft.AspNetCore.Identity;

namespace TicketManagementApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }

}
