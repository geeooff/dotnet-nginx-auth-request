using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using App.Options;
using App.Services;

namespace App.Extensions.DependencyInjection
{
	public class SeedDataBuilder : ISeedDataBuilder
	{
		private readonly ILogger<SeedDataBuilder> _logger;
		private readonly SeedDataOptions _options;
		private readonly ISeedDataService _service;

		public SeedDataBuilder(IServiceCollection services)
		{
			var serviceProvider = services.BuildServiceProvider();

			_logger = serviceProvider.GetRequiredService<ILogger<SeedDataBuilder>>();
			_options = serviceProvider.GetRequiredService<IOptions<SeedDataOptions>>().Value;

			if (_options.IsEnabled)
			{
				_service = serviceProvider.GetRequiredService<ISeedDataService>();
			}
		}

		public SeedDataOptions Options => _options;
		public ISeedDataService Service => _service;

		public void AddRolesAndUsers()
		{
			if (_options.IsEnabled)
			{
				_service.AddRolesAsync().Wait();
				_service.AddUsersAsync().Wait();
			}
			else
			{
				_logger.LogDebug("Seed data service is disabled");
			}
		}
	}
}
