using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;

namespace SqlCollector.Models
{
	public class SqlResourceDto
	{
		public string ServerNameId { get; set; }
		public string ServerName { get; set; }
		public string SubscriptionId { get; set; }
		public string AdminLogin { get; set; }
		public List<SqlServerDatabaseDto> Databases { get; set; }
		public DateTime? CreatedOn { get; set; }

		public SqlResourceDto()
		{
			Databases = new List<SqlServerDatabaseDto>();
		}
	}
}


