using System.Collections.Generic;
using System.Threading.Tasks;
using SqlCollector.Models;

namespace SqlCollector.Services
{
	public interface ISqlResourceInfoInventoryService
	{
		SqlResourceDto GetSqlServerResource(ResourceDto _sqlResource);
		Task<SqlResourceDto> GetSqlServerResourceAsync(ResourceDto _sqlResource);
		Task<SqlResourceDto> GetSqlVirtualMachineResourceAsync(ResourceDto _sqlResource); 
	}
}
