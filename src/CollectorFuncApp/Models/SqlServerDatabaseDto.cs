using System;
using Microsoft.Azure.Cosmos.Table;

namespace SqlCollector.Models
{
	public class SqlServerDatabaseDto
	{
		public string ServerNameId { get; set; }
		public string DatabaseName { get; set; }
		public string ServerName { get; set; }
		public string SubscriptionId { get; set; }
		public DateTime? CreatedOn { get; set; }
		
	}
}


