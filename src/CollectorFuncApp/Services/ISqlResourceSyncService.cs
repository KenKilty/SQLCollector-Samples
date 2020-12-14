using System.Data;
using System.Threading.Tasks;

namespace SqlCollector.Services
{
	public interface ISqlResourceSyncService
	{
		Task SyncSqlResourcesToAzSQL(DataTable stageDataTable);
		Task SyncSqlResourcesToAzSQLAsync(DataTable stageDataTable);
	}
}
