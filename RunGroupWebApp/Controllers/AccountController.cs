using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RunGroupWebApp.Data;
using RunGroupWebApp.Interfaces;
using RunGroupWebApp.Models;
using RunGroupWebApp.ViewModels;

namespace RunGroupWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILocationService _locationService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ApplicationDbContext context,
            ILocationService locationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _locationService = locationService;
        }
        public IActionResult Login()
        {
            var response = new LoginViewModel();
            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(loginViewModel);
            }
            var user = await _userManager.FindByEmailAsync(loginViewModel.EmailAddress);
            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(loginViewModel);
            }
            var passwordCheck = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);
            if(!passwordCheck)
            {
                ModelState.AddModelError(string.Empty, "Invalid password attempt.");
                return View(loginViewModel);
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
            if(result.Succeeded)
            {
                return RedirectToAction("Index", "Race");
            }
            return View(loginViewModel);
        }
        [HttpGet]
        public IActionResult Register()
        {
            var response = new RegisterViewModel();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }
            var user = await _userManager.FindByEmailAsync(registerViewModel.EmailAddress);
            if (user != null)
            {
                ModelState.AddModelError(string.Empty, "User already exists.");
                return View(registerViewModel);
            }
            var newUser = new AppUser
            {
                UserName = registerViewModel.EmailAddress,
                Email = registerViewModel.EmailAddress
            };
            var result = await _userManager.CreateAsync(newUser, registerViewModel.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser,UserRoles.User);
            }
            return RedirectToAction("Index","Race");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Race");
        }

        [HttpGet]
        [Route("Account/Welcome")]
        public async Task<IActionResult> Welcome(int page = 0)
        {
            if (page == 0)
            {
                return View();
            }
            return View();

        }

        [HttpGet]
        public async Task<IActionResult> GetLocation(string location)
        {
            if (location == null)
            {
                return Json("Not found");
            }
            var locationResult = await _locationService.GetLocationSearch(location);
            return Json(locationResult);
        }

    }
}
