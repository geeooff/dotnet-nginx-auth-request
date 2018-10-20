using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Options
{
	public class SeedDataOptions
	{
		public class User
		{
			public string Name { get; set; }
			public string Password { get; set; }
			public IList<string> Roles { get; set; }
		}

		public bool IsEnabled { get; set; }
		public IList<User> Users { get; set; }
		public IList<string> Roles { get; set; }
	}
}
