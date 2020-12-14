using System;
using Microsoft.Azure.Cosmos.Table;

namespace SqlCollector.Models
{
	public class SqlServerDatabaseEntity : TableEntity
	{
		public string SubscriptionId { get; set; }
		public string ServerName { get; set; }
		public DateTime? CreatedOn { get; set; }
		public DateTime LastSeenOn { get; set; }
	}
}


