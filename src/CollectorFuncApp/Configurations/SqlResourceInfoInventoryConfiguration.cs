
using System;

namespace SqlCollector.Configurations
{
	public class SqlResourceInfoInventoryConfiguration
	{
		public bool UseSystemAssignedManagedIdentity { get; set; } = true;

		public string UserAssignedManagedIdentityClientId { get; set; } = Guid.Empty.ToString();
	}
}
