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
using Microsoft.AspNetCore.HttpOverrides;

namespace App
{
	public class Startup
	{
		private AppOptions _appOptions;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			string redisConnectionString = Configuration.GetConnectionString("RedisConnection");

			var appConfig = Configuration.GetSection("App");
			var identityConfig = appConfig.GetSection("Identity");
			var antiForgeryConfig = appConfig.GetSection("AntiForgery");
			var authCookieConfig = appConfig.GetSection("AuthCookie");
			var seedDataConfig = appConfig.GetSection("SeedData");

			_appOptions = new AppOptions();
			appConfig.Bind(_appOptions);

			services
				.AddSingleton(_appOptions);

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

					options.LoginPath = $"{_appOptions.BasePath}/login";
					options.LogoutPath = $"{_appOptions.BasePath}/logout";
					options.AccessDeniedPath = $"{_appOptions.BasePath}/error";
					options.ReturnUrlParameter = "then";
				});

			services
				.AddSeedData(options => seedDataConfig.Bind(options))
				.AddRolesAndUsers();

			services
				.AddAntiforgery(options =>
				{
					antiForgeryConfig.Bind(options);					
				});

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
				app.UseExceptionHandler($"{_appOptions.BasePath}/error");
			}

			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.All
			});

			app.UseStaticFiles($"{_appOptions.BasePath}");

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute("auth-request", $"{_appOptions.BasePath}/request", new { controller = "Auth", action = nameof(AuthController.AuthRequest) });
				routes.MapRoute("auth-login", $"{_appOptions.BasePath}/login", new { controller = "Auth", action = nameof(AuthController.Login) });
				routes.MapRoute("auth-logout", $"{_appOptions.BasePath}/logout", new { controller = "Auth", action = nameof(AuthController.Logout) });
				routes.MapRoute("auth-error", $"{_appOptions.BasePath}/error", new { controller = "Auth", action = nameof(AuthController.Error) });
			});
		}
	}
}
