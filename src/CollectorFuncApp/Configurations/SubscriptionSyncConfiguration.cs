
using System;

namespace SqlCollector.Configurations
{
	public class SubscriptionSyncConfiguration
	{
		public string SQLCollectorDbConnectionString { get; set; } = Guid.Empty.ToString();
	}
}
