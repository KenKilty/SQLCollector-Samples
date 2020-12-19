using System.Data;
using System.Threading.Tasks;

namespace SqlCollector.Services
{
	public interface ISqlResourceDatabaseSyncService
	{
		Task SyncSqlResourceDatabasesToAzSQL(DataTable stageDataTable);
		Task SyncSqlResourceDatabasesToAzSQLAsync(DataTable stageDataTable);
	}
}
