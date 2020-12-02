using System;
using Microsoft.Azure.Cosmos.Table;

namespace SqlCollector.Models
{
	public class SubscriptionEntity : TableEntity
	{
		public string SubscriptionName { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime LastSeenOn { get; set; }
	}
}


