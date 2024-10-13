using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using Web_Chat.Models;
using Web_Chat.Models.Form;

namespace Web_Chat.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManger;

        public AccountController(UserManager<User> userManager)
        {
            _userManger = userManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnPath)
        {
            return View(new LoginForm());
        }

        [HttpGet]
        public IActionResult Register(string? returnPath)
        {
            return View(new RegisterForm());
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterForm form,string? returnPath)
        {
            if (!ModelState.IsValid)
            {
                return View(form);
            }

            if (form.Password != form.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(form.ConfirmPassword),"Password mismatch");
                return View(form);
            }

            if (null != await _userManger.FindByEmailAsync(form.Email))
            {
                ModelState.AddModelError(nameof(form.Email),"User with " + form.Email + " already exists");
                return View(form);
            }

            var user = new User
            {
                Email = form.Email,
                FirstName = form.FirstName,
                LastName = form.LastName,
                UserName = form.Email
            };

            var result = await _userManger.CreateAsync(user,form.Password);


            await SignIn(user);

            if (returnPath != null)
            {
                return Redirect(returnPath);
            }

            return RedirectToAction("Index","Home");
        }


		[HttpPost]
		public async Task<IActionResult> Login([FromForm] LoginForm form, string? returnPath)
		{
			if (!ModelState.IsValid)
			{
				return View(form);
			}

			var user = await _userManger.FindByEmailAsync(form.Email);
			if (user == null)
			{
				ModelState.AddModelError(nameof(form.Email), "User with this email not found");
				return View(form);
			}

			if (!await _userManger.CheckPasswordAsync(user, form.Password))
			{
				ModelState.AddModelError(nameof(form.Password), "Invalid password");
				return View(form);
			}

			await SignIn(user);

			if (!string.IsNullOrEmpty(returnPath) && returnPath.StartsWith("/"))
			{
				return Redirect(returnPath);
			}

			return RedirectToAction("Index", "Home");
		}





		[HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        public IActionResult AccessDenied(string? returnPath)
        {
            ViewData["ReturnUrl"] = returnPath;
            return View();
        }

        private async Task SignIn(User user)
        {
            var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            identity.AddClaim(new Claim("FirstName", user.FirstName ?? string.Empty));
            identity.AddClaim(new Claim("LastName", user.LastName ?? string.Empty));

            var userRoles = await _userManger.GetRolesAsync(user);
            userRoles.ToList().ForEach(r =>
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, r));
            });

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);
        }
    }
}
