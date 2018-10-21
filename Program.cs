using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseKestrel(options =>
				{
					options.AddServerHeader = false;
				})
				.ConfigureAppConfiguration((hostingContext, options) =>
				{
					options.AddJsonFile("seeddata.json", optional: true, reloadOnChange: false);
				})
				.UseStartup<Startup>();
	}
}
