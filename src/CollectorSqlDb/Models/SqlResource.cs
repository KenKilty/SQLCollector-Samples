using System;
using System.Collections.Generic;

#nullable disable

namespace SqlCollectorDb.Models
{
	public partial class SqlResource
	{
		public int Id { get; set; }
		public string ResourceId { get; set; }
		public string Name { get; set; }
		public string AdminLogin { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime LastSeenOn { get; set; }
	}
}
