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
	public class SqlResourceSync
	{
		private readonly ISqlResourceSyncService _sqlResourceSyncService;

		public SqlResourceSync(ISqlResourceSyncService sqlResourceSyncService)
		{
			_sqlResourceSyncService = sqlResourceSyncService ?? throw new ArgumentNullException(nameof(ISqlResourceSyncService));
		}

		[FunctionName("SqlResourceSync")]
		public async Task Run(
			[TimerTrigger("%SqlResourceSyncSchedule%")] TimerInfo timerInfo,
			[Table("InventorySqlResource", Connection = "StorageConnectionAppSetting")] CloudTable sqlInfoTable,
			ILogger log)
		{
			log.LogInformation($"C# Sql Resource Sync Timer trigger function executed at: {DateTime.UtcNow}. Next occurrence: {timerInfo.FormatNextOccurrences(1)}");

			DataTable stageDataTable = new DataTable
			{
				Columns = { { "ID", typeof(int) }, { "ResourceId", typeof(string) }, { "Name", typeof(string) }, { "AdminLogin", typeof(string) }, { "Type", typeof(string) }, { "CreatedOn", typeof(DateTime) }, { "LastSeenOn", typeof(DateTime) } }
			};
			stageDataTable.PrimaryKey = new[] { stageDataTable.Columns[0] };

			TableContinuationToken token = null;
			do
			{
				TableQuerySegment<SqlResourceEntity> queryResult = await sqlInfoTable.ExecuteQuerySegmentedAsync(new TableQuery<SqlResourceEntity>(), token);
				int index = 1;

				foreach (SqlResourceEntity result in queryResult)
				{
					if (Guid.TryParse(result.PartitionKey, out Guid subscriptionId))
						log.LogInformation($"Converted Subscription Id {result.PartitionKey} to a Guid");
					else {
						log.LogWarning($"Unable to convert Subscription Id {result.PartitionKey} to a Guid. Skipping the entity.");
						break;
					}

					DataRow row = stageDataTable.NewRow();
					row["ID"] = index;
					row["ResourceId"] = result.ServerNameId;
					row["Name"] = result.RowKey;
					row["AdminLogin"] = result.AdminLogin;
					row["Type"] = result.Type;
					row["CreatedOn"] = result.CreatedOn;
					row["LastSeenOn"] = result.LastSeenOn;

					stageDataTable.Rows.Add(row);
					index++;
				}

				token = queryResult.ContinuationToken;
			} while (token != null);

			await _sqlResourceSyncService.SyncSqlResourcesToAzSQLAsync(stageDataTable);
		}
	}
}
