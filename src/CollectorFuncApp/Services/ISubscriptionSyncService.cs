using System.Data;
using System.Threading.Tasks;

namespace SqlCollector.Services
{
	public interface ISubscriptionSyncService
	{
		Task SyncSubscriptionsToAzSQL(DataTable stageDataTable);
		Task SyncSubscriptionsToAzSQLAsync(DataTable stageDataTable);
	}
}
