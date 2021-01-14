using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Cosmos.Table;
using SqlCollector.Models;
using SqlCollector.Tools;
using SqlCollector.Services;

namespace SqlCollector.Functions
{
	public class SubscriptionCollector
	{
		private readonly ISubscriptionInventoryService _subscriptionInventoryService;

		public SubscriptionCollector(ISubscriptionInventoryService subscriptionInventoryService)
		{
			 _subscriptionInventoryService = subscriptionInventoryService ?? throw new ArgumentNullException(nameof(ISubscriptionInventoryService));
		}

		[FunctionName("SubscriptionInventory")]
		public async Task Run(
			[TimerTrigger("%SubscriptionInventorySchedule%")] TimerInfo timerInfo,
			[Table("InventorySubscription", Connection = "StorageConnectionAppSetting")] CloudTable inventorySubscription,
			[Queue("outqueue"), StorageAccount("StorageConnectionAppSetting")] ICollector<string> msg,
			ILogger log)
		{
			log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}. Next occurrence: {timerInfo.FormatNextOccurrences(1)}");

			IReadOnlyList<SubscriptionDto> subscriptions = await _subscriptionInventoryService.GetSubscriptionsAsync();

			foreach (SubscriptionDto sub in subscriptions)
			{
				await inventorySubscription.CreateIfNotExistsAsync();
				DateTime init = DateTime.Now;

				SubscriptionEntity subEntity = new SubscriptionEntity()
				{
					PartitionKey = sub.SubscriptionId,
					RowKey = Constants.SubscriptionEntitySummaryRowKey,
					SubscriptionName = sub.SubscriptionName,
					CreatedOn = init,
					LastSeenOn = init
				};

				TableOperation retrieveOperation = TableOperation.Retrieve<SubscriptionEntity>(sub.SubscriptionId, Constants.SubscriptionEntitySummaryRowKey);
				TableResult retrievedEntity = await inventorySubscription.ExecuteAsync(retrieveOperation);

				if (retrievedEntity.Result == null)
				{
					TableOperation insertOperation = TableOperation.Insert(subEntity);
					await inventorySubscription.ExecuteAsync(insertOperation);
				}
				else
				{
					subEntity.CreatedOn = ((SubscriptionEntity)retrievedEntity.Result).CreatedOn;
					TableOperation mergeOperation = TableOperation.InsertOrMerge(subEntity);
					await inventorySubscription.ExecuteAsync(mergeOperation);
				}

				msg.Add(sub.SubscriptionId);
			}
		}
	}
}
