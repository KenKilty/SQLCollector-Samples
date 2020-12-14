using System;
using Microsoft.Azure.Cosmos.Table;

namespace SqlCollector.Models
{
	public class SqlResourceEntity : TableEntity
	{
		public string ServerName { get; set; }
		public string AdminLogin { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime LastSeenOn { get; set; }
	}
}


