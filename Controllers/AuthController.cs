using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using App.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace App.Controllers
{
	public class AuthController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger _logger;

		private string _loggerPrefix;

		public AuthController(
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			ILogger<AuthController> logger)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			_loggerPrefix = string.Concat(
				HttpContext.Connection.LocalIpAddress,
				": "
			);

			base.OnActionExecuting(context);
		}

		[HttpGet]
		public async Task<IActionResult> AuthRequest(string role = null)
		{
			if (User.Identity.IsAuthenticated)
			{
				var user = await _userManager.GetUserAsync(User);

				if (user == null || await _userManager.IsLockedOutAsync(user))
				{
					_logger.LogWarning($"{_loggerPrefix}Authorization failure: user is unknown or locked out");

					await HttpContext.SignOutAsync();

					return Forbid();
				}
				else if (!string.IsNullOrWhiteSpace(role) && !await _userManager.IsInRoleAsync(user, role))
				{
					_logger.LogWarning($"{_loggerPrefix}Authorization failure: user is not member of {role} role");

					return Forbid();
				}
				else
				{
					_logger.LogTrace($"{_loggerPrefix}Authentication success");

					var roles = await _userManager.GetRolesAsync(user);

					Response.Headers["X-Forwarded-User"] = user.UserName;
					Response.Headers["X-Forwarded-Roles"] = string.Join(",", roles);

					return Ok();
				}
			}
			else
			{
				_logger.LogDebug($"{_loggerPrefix}Authentication failure: unauthorized");

				return Unauthorized();
			}
		}

		[HttpGet]
		public IActionResult Login(string then = null)
		{
			ViewData["then"] = then;

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string then = null)
		{
			ViewData["then"] = then;

			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(
					model.Username,
					model.Password,
					isPersistent: true,
					lockoutOnFailure: true
				);

				if (result.Succeeded)
				{
					_logger.LogInformation($"{_loggerPrefix}Login success for user {model.Username}");

					if (Url.IsLocalUrl(then))
						return Redirect(then);
					else
						return RedirectToAction(nameof(Login));
				}
				else
				{
					_logger.LogWarning($"{_loggerPrefix}Login failure for user {model.Username}. IsLockedOut = {result.IsLockedOut}, IsNotAllowed = {result.IsNotAllowed}");

					ModelState.AddModelError(string.Empty, "Invalid username or password");

					return View(model);
				}
			}
			else
			{
				_logger.LogWarning($"{_loggerPrefix}Login form validation failure");
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();

			_logger.LogInformation($"{_loggerPrefix}User logged out");

			return RedirectToAction(nameof(Login));
		}

		[HttpGet]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}