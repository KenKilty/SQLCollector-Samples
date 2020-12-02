
using System;

namespace SqlCollector.Configurations
{
	public class SubscriptionInventoryConfiguration
	{
		public bool UseSystemAssignedManagedIdentity { get; set; } = true;

		public string UserAssignedManagedIdentityClientId { get; set; } = Guid.Empty.ToString();
	}
}
