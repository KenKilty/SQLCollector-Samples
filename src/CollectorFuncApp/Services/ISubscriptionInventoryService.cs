using System.Collections.Generic;
using System.Threading.Tasks;
using SqlCollector.Models;

namespace SqlCollector.Services
{
	public interface ISubscriptionInventoryService
	{
		IReadOnlyList<SubscriptionDto> GetSubscriptions();
		Task<IReadOnlyList<SubscriptionDto>> GetSubscriptionsAsync();
	}
}
