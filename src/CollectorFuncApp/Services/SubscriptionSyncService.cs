using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SqlCollector.Configurations;

namespace SqlCollector.Services
{
	public class SubscriptionSyncService : ISubscriptionSyncService
	{
		private readonly SubscriptionSyncConfiguration _subscriptionSyncConfiguration;

		public SubscriptionSyncService(IOptions<SubscriptionSyncConfiguration> subscriptionSyncConfiguration)
		{
			_subscriptionSyncConfiguration = subscriptionSyncConfiguration.Value;
		}

		public Task SyncSubscriptionsToAzSQL(DataTable stageDataTable)
		{
			SyncSubscriptionsToAzSQLAsync(stageDataTable).RunSynchronously();
			return Task.CompletedTask;
		}

		public async Task SyncSubscriptionsToAzSQLAsync(DataTable stageDataTable)
		{
			using SqlConnection destinationConnection = new SqlConnection(_subscriptionSyncConfiguration.SQLCollectorDbConnectionString);
			await destinationConnection.OpenAsync();
			using SqlCommand truncateStageCommand = new SqlCommand("TRUNCATE TABLE [app].[SubscriptionStage]", destinationConnection);
			using SqlCommand upsertStageCommand = new SqlCommand("EXECUTE [app].[uspSubscriptionStageUpsert]", destinationConnection);
			using SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection)
			{
				DestinationTableName = "[app].[SubscriptionStage]"
			};
			_ = await truncateStageCommand.ExecuteNonQueryAsync();
			await bulkCopy.WriteToServerAsync(stageDataTable);
			_ = await upsertStageCommand.ExecuteNonQueryAsync();
		}
	}
}
