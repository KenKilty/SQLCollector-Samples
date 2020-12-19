﻿using System;
using System.Collections.Generic;

#nullable disable

namespace SqlCollectorDb.Models
{
	public partial class SqlResourceDatabaseStage
	{
		public int Id { get; set; }
		public string ServerNameId { get; set; }
		public string Name { get; set; }
		public string ServerName { get; set; }
		public string SubscriptionId { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime LastSeenOn { get; set; }
	}
}
