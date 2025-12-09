//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using TicketManagementApp.Models;
//using TicketManagementApp.Models.ViewModels;
//using TicketManagementApp.Data;

//namespace TicketManagementApp.Controllers
//{
//    public class UsersController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly ApplicationDbContext _dbContext;

//        public UsersController(
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager, ApplicationDbContext dbContext)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _dbContext = dbContext;
//        }

//        // GET: Users
//        [Authorize]
//        public IActionResult Index()
//        {
//            return View();
//        }

//        // GET: Users/Login
//        [HttpGet]
//        public IActionResult Login(string returnUrl = null)
//        {
//            if (User.Identity.IsAuthenticated)
//            {
//                return RedirectToAction("Index", "Tickets");
//            }

//            ViewData["ReturnUrl"] = returnUrl;
//            return View();
//        }

//        // POST: Users/Login
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            if (ModelState.IsValid)
//            {
//                // Tenter de se connecter avec l'email
//                var result = await _signInManager.PasswordSignInAsync(
//                    model.Email,
//                    model.Password,
//                    model.RememberMe,
//                    lockoutOnFailure: false);

//                if (result.Succeeded)
//                {
//                    // Rediriger vers l'URL de retour ou vers la page des tickets
//                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
//                    {
//                        return Redirect(returnUrl);
//                    }
//                    return RedirectToAction("Index", "Tickets");
//                }
//                else
//                {
//                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
//                }
//            }

//            return View(model);
//        }

//        // GET: Users/Register
//        [HttpGet]
//        public IActionResult Register(string returnUrl = null)
//        {
//            if (User.Identity.IsAuthenticated)
//            {
//                return RedirectToAction("Index", "Tickets");
//            }

//            ViewData["ReturnUrl"] = returnUrl;
//            return View();
//        }

//        // POST: Users/Register
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;

//            if (ModelState.IsValid)
//            {
//                var user = new ApplicationUser
//                {
//                    UserName = model.Email,
//                    Email = model.Email,
//                    FullName = model.FullName
//                };

//                var result = await _userManager.CreateAsync(user, model.Password);

//                if (result.Succeeded)
//                {
//                    // Connecter automatiquement l'utilisateur après l'inscription
//                    await _signInManager.SignInAsync(user, isPersistent: false);

//                    // Rediriger vers l'URL de retour ou vers la page des tickets
//                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
//                    {
//                        return Redirect(returnUrl);
//                    }
//                    return RedirectToAction("Index", "Tickets");
//                }

//                // Ajouter les erreurs au ModelState
//                foreach (var error in result.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, error.Description);
//                }
//            }

//            return View(model);
//        }

//        // POST: Users/Logout
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return RedirectToAction("Login", "Users");
//        }

//        // GET: Users/AccessDenied
//        [HttpGet]
//        public IActionResult AccessDenied()
//        {
//            return View();
//        }

//        // GET: Users/Profile
//        [HttpGet]
//        [Authorize]
//        public async Task<IActionResult> Profil()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//            {
//                return RedirectToAction("Login");
//            }

//            var userId = user.Id;

//            // Récupérer les tickets créés par l'utilisateur
//            var createdTickets = await _dbContext.Tickets
//                .Include(t => t.AssignedToUser)
//                .Include(t => t.CreatedByUser)
//                .Where(t => t.CreatedByUserId == userId)
//                .OrderByDescending(t => t.CreatedAt)
//                .ToListAsync();

//            // Récupérer les tickets assignés à l'utilisateur
//            var assignedTickets = await _dbContext.Tickets
//                .Include(t => t.AssignedToUser)
//                .Include(t => t.CreatedByUser)
//                .Where(t => t.AssignedToUserId == userId)
//                .OrderByDescending(t => t.CreatedAt)
//                .ToListAsync();

//            var viewModel = new ProfilViewModel
//            {
//                UserId = userId,
//                FullName = user.FullName,
//                Email = user.Email,
//                CreatedTickets = createdTickets,
//                AssignedTickets = assignedTickets
//            };

//            return View(viewModel);
//        }
//    }
//}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketManagementApp.Models;
using TicketManagementApp.Models.ViewModels;
using TicketManagementApp.Services;

namespace TicketManagementApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITicketService _ticketService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITicketService ticketService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _ticketService = ticketService;
        }

        // GET: Users
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        // GET: Users/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Tickets");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Tickets");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.  Please check your email and password.");
                }
            }

            return View(model);
        }

        // GET:  Users/Register
        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Tickets");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Tickets");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: Users/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Users");
        }

        // GET: Users/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Users/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profil()
        {
            var user = await _ticketService.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var viewModel = await _ticketService.GetUserProfileAsync(user.Id);
            if (viewModel == null)
            {
                return RedirectToAction("Login");
            }

            return View(viewModel);
        }
    }
}