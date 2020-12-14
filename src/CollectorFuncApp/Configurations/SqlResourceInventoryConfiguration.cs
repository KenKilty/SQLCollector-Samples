
using System;

namespace SqlCollector.Configurations
{
	public class SqlResourceInventoryConfiguration
	{
		public bool UseSystemAssignedManagedIdentity { get; set; } = true;

		public string UserAssignedManagedIdentityClientId { get; set; } = Guid.Empty.ToString();
	}
}
