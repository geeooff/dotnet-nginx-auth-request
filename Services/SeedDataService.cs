using App.Models;
using App.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services
{
	public class SeedDataService : ISeedDataService
	{
		public readonly ILogger<SeedDataService> _logger;
		public readonly SeedDataOptions _options;
		public readonly UserManager<User> _userManager;
		public readonly RoleManager<Role> _roleManager;

		public SeedDataService(
			ILogger<SeedDataService> logger,
			IOptions<SeedDataOptions> optionsAccessor,
			UserManager<User> userManager,
			RoleManager<Role> roleManager)
		{
			_logger = logger;
			_options = optionsAccessor.Value;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public async Task AddRolesAsync()
		{
			if (_options.IsEnabled)
			{
				if (_options.Roles?.Any() ?? false)
				{
					foreach (string rolename in _options.Roles)
					{
						await AddRoleAsync(rolename);
					}
				}
				else
				{
					_logger.LogWarning("Seed data contains no role");
				}
			}
			else
			{
				_logger.LogWarning("Seed data service is disabled");
			}
		}

		public async Task AddUsersAsync()
		{
			if (_options.IsEnabled)
			{
				if (_options.Users?.Any() ?? false)
				{
					foreach (var user in _options.Users)
					{
						await AddUserAsync(user);
					}
				}
				else
				{
					_logger.LogWarning("Seed data contains no user");
				}
			}
			else
			{
				_logger.LogWarning("Seed data service is disabled");
			}
		}

		private async Task AddRoleAsync(string role)
		{
			if (string.IsNullOrWhiteSpace(role))
			{
				_logger.LogWarning("Seed data contain a blank role name. It will be ignored");
			}
			else
			{
				_logger.LogDebug($"Searching role {role}...");

				if (await _roleManager.RoleExistsAsync(role))
				{
					_logger.LogDebug($"Role {role} already exists");
				}
				else
				{
					_logger.LogDebug($"Role {role} don't exists");

					var dbRole = new Role()
					{
						Name = role
					};

					_logger.LogDebug($"Role {role} is creating...");

					var roleCreationResult = await _roleManager.CreateAsync(dbRole);

					_logger.Log(
						roleCreationResult.Succeeded ? LogLevel.Information : LogLevel.Error,
						$"Role {role} (id = {dbRole.Id}) creation result: {roleCreationResult}"
					);
				}
			}
		}

		private async Task AddUserAsync(SeedDataOptions.User user)
		{
			if (!string.IsNullOrWhiteSpace(user.Name))
			{
				_logger.LogDebug($"Searching for user {user.Name}...");

				User dbUser = await _userManager.FindByNameAsync(user.Name);

				if (dbUser == null)
				{
					_logger.LogDebug($"User {user.Name} don't exists");

					dbUser = new User()
					{
						UserName = user.Name
					};

					_logger.LogDebug($"User {user.Name} is creating...");

					var userCreationResult = await _userManager.CreateAsync(dbUser);

					_logger.Log(
						userCreationResult.Succeeded ? LogLevel.Information : LogLevel.Error,
						$"User {user.Name} (id = {dbUser.Id}) creation result: {userCreationResult}"
					);
				}
				else
				{
					_logger.LogDebug($"User {user.Name} (id = {dbUser.Id}) already exists");
				}

				await AddPasswordAsync(dbUser, user.Password);
				await AddUserToRolesAsync(dbUser, user.Roles);
			}
			else
			{
				_logger.LogWarning("Seed data contain a blank user name. It will be ignored");
			}
		}

		private async Task AddPasswordAsync(User user, string password)
		{
			if (!string.IsNullOrWhiteSpace(password))
			{
				_logger.LogDebug($"Checking if user {user.UserName} (id = {user.Id}) have password...");

				if (await _userManager.HasPasswordAsync(user))
				{
					_logger.LogDebug($"User {user.UserName} (id = {user.Id}) already has password");
				}
				else
				{
					_logger.LogDebug($"User {user.UserName} (id = {user.Id}) has no password. Creating...");

					var passwordCreationResult = await _userManager.AddPasswordAsync(user, password);

					_logger.Log(
						passwordCreationResult.Succeeded ? LogLevel.Information : LogLevel.Error,
						$"User {user.UserName} (id = {user.Id}) password creation result: {passwordCreationResult}"
					);
				}
			}
			else
			{
				_logger.LogWarning($"No password have been specified for user {user.UserName}. It will be ignored");
			}
		}

		private async Task AddUserToRolesAsync(User user, IList<string> roles)
		{
			if (roles?.Any() ?? false)
			{
				_logger.LogDebug($"Getting user {user.UserName} (id = {user.Id}) roles...");

				var dbRoles = await _userManager.GetRolesAsync(user);

				var rolesToAdd = roles.Except(dbRoles);

				if (rolesToAdd.Any())
				{
					if (dbRoles.Any())
						_logger.LogDebug($"User {user.UserName} (id = {user.Id}) is already in roles: {string.Join(", ", dbRoles)}. Adding him to roles: {string.Join(", ", rolesToAdd)}...");
					else
						_logger.LogDebug($"User {user.UserName} (id = {user.Id}) has no role. Adding him to roles: {string.Join(", ", rolesToAdd)}...");

					var rolesMembershipResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

					_logger.Log(
						rolesMembershipResult.Succeeded ? LogLevel.Information : LogLevel.Error,
						$"User {user.UserName} (id = {user.Id}) roles membership result: {rolesMembershipResult}"
					);
				}
				else
				{
					_logger.LogWarning($"User {user.UserName} (id = {user.Id}) is already in all roles: {string.Join(", ", dbRoles)}");
				}
			}
			else
			{
				_logger.LogWarning($"No role have been specified for user {user.UserName}. They will be ignored");
			}
		}
	}
}
