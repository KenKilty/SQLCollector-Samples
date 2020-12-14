using System.Collections.Generic;
using System.Threading.Tasks;
using SqlCollector.Models;

namespace SqlCollector.Services
{
	public interface ISqlResourceInventoryService
	{
		IReadOnlyList<ResourceDto> GetSqlResources(string _subscriptionId);
		Task<IReadOnlyList<ResourceDto>> GetSqlResourcesAsync(string _subscriptionId);
	}
}
