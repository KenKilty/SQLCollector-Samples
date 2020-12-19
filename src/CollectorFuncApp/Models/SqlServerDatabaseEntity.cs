using System;
using Microsoft.Azure.Cosmos.Table;

namespace SqlCollector.Models
{
	public class SqlServerDatabaseEntity : TableEntity
	{
		public string SubscriptionId { get; set; }
		public string ServerNameId { get; set; }
		public DateTime? CreatedOn { get; set; }
		public DateTime LastSeenOn { get; set; }
	}
}


