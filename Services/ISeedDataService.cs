using System.Threading.Tasks;

namespace App.Services
{
	public interface ISeedDataService
	{
		Task AddRolesAsync();
		Task AddUsersAsync();
	}
}