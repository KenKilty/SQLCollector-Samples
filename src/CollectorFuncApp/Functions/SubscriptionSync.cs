using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Cosmos.Table;
using System.Data;
using SqlCollector.Models;
using SqlCollector.Services;

namespace SqlCollector.Functions
{
	public class SubscriptionSync
	{
		private readonly ISubscriptionSyncService _subscriptionSyncService;

		public SubscriptionSync(ISubscriptionSyncService subscriptionSyncService)
		{
			_subscriptionSyncService = subscriptionSyncService ?? throw new ArgumentNullException(nameof(ISubscriptionSyncService));
		}

		[FunctionName("SubscriptionSync")]
		public async Task Run(
			[TimerTrigger("%SubscriptionSyncSchedule%")] TimerInfo timerInfo,
			[Table("InventorySubscription", Connection = "StorageConnectionAppSetting")] CloudTable inventorySubscription,
			ILogger log)
		{
			log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}. Next occurrence: {timerInfo.FormatNextOccurrences(1)}");

			DataTable stageDataTable = new DataTable
			{
				Columns = { { "ID", typeof(Guid) }, { "Name", typeof(string) }, { "CreatedOn", typeof(DateTime) }, { "LastSeenOn", typeof(DateTime) } }
			};
			stageDataTable.PrimaryKey = new[] { stageDataTable.Columns[0] };

			TableContinuationToken token = null;
			do
			{
				TableQuerySegment<SubscriptionEntity> queryResult = await inventorySubscription.ExecuteQuerySegmentedAsync(new TableQuery<SubscriptionEntity>(), token);

				foreach (SubscriptionEntity result in queryResult)
				{
					if (Guid.TryParse(result.PartitionKey, out Guid subscriptionId))
						log.LogInformation($"Converted Subscription Id {result.PartitionKey} to a Guid");
					else {
						log.LogWarning($"Unable to convert Subscription Id {result.PartitionKey} to a Guid. Skipping the entity.");
						break;
					}

					DataRow row = stageDataTable.NewRow();
					row["ID"] = subscriptionId;
					row["Name"] = result.SubscriptionName;
					row["CreatedOn"] = result.CreatedOn;
					row["LastSeenOn"] = result.LastSeenOn;

					stageDataTable.Rows.Add(row);
				}

				token = queryResult.ContinuationToken;
			} while (token != null);

			await _subscriptionSyncService.SyncSubscriptionsToAzSQLAsync(stageDataTable);
		}
	}
}
