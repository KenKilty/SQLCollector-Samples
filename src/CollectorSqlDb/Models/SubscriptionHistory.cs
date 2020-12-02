using System;
using System.Collections.Generic;

#nullable disable

namespace SqlCollectorDb.Models
{
	public partial class SubscriptionHistory
	{
		public int HistoryId { get; set; }
		public Guid Id { get; set; }
		public string Name { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime LastSeenOn { get; set; }
		public DateTime ArchivedOn { get; set; }
	}
}
