
using System;

namespace SqlCollector.Configurations
{
	public class SqlResourceSyncConfiguration
	{
		public string SQLCollectorDbConnectionString { get; set; } = Guid.Empty.ToString();
	}
}
