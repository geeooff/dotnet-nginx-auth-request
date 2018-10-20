using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using App.Extensions.DependencyInjection;
using App.Options;
using App.Services;
using Microsoft.Extensions.Options;

namespace App.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static SeedDataBuilder AddSeedData(this IServiceCollection services, Action<SeedDataOptions> setupAction)
		{
			services
				.AddOptions<SeedDataOptions>()
				.Configure(setupAction);

			var serviceProvider = services.BuildServiceProvider();
			var optionsAccessor = serviceProvider.GetRequiredService<IOptions<SeedDataOptions>>();
			var options = optionsAccessor.Value;

			if (options.IsEnabled)
			{
				services.AddSingleton<ISeedDataService, SeedDataService>();
			}

			return new SeedDataBuilder(services);
		}
	}
}
