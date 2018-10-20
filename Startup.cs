using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using App.Models;
using App.Controllers;
using App.Extensions;
using App.Options;

namespace App
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			string redisConnectionString = Configuration.GetConnectionString("RedisConnection");

			var authCookieConfig = Configuration.GetSection("AuthCookie");
			var identityConfig = Configuration.GetSection("Identity");
			var seedDataConfig = Configuration.GetSection("SeedData");

			services
				.AddIdentity<User, Role>(options => identityConfig.Bind(options))
				.AddRedisStores(redisConnectionString)
				.AddDefaultTokenProviders();

			services
				.ConfigureApplicationCookie(options =>
				{
					authCookieConfig.Bind(options.Cookie);

					if (options.Cookie.Expiration.HasValue)
					{
						options.ExpireTimeSpan = options.Cookie.Expiration.Value;
					}

					options.LoginPath = "/auth/login";
					options.LogoutPath = "/auth/logout";
					options.AccessDeniedPath = "/auth/error";
					options.ReturnUrlParameter = "then";
				});

			services
				.AddSeedData(options => seedDataConfig.Bind(options))
				.AddRolesAndUsers();

			services
				.AddMvc()
				.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/auth/error");
			}

			app.UseHttpsRedirection();

			app.UseStaticFiles("/auth-assets");

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute("auth-request", "auth/request", new { controller = "Auth", action = nameof(AuthController.AuthRequest) });
				routes.MapRoute("auth-login", "auth/login", new { controller = "Auth", action = nameof(AuthController.Login) });
				routes.MapRoute("auth-logout", "auth/logout", new { controller = "Auth", action = nameof(AuthController.Logout) });
				routes.MapRoute("auth-error", "auth/error", new { controller = "Auth", action = nameof(AuthController.Error) });
			});
		}
	}
}
