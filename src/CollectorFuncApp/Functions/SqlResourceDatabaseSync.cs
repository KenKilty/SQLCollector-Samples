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
	public class SqlResourceDatabaseSync
	{
		private readonly ISqlResourceDatabaseSyncService _sqlResourceDatabaseSyncService;

		public SqlResourceDatabaseSync(ISqlResourceDatabaseSyncService sqlResourceDatabaseSyncService)
		{
			_sqlResourceDatabaseSyncService = sqlResourceDatabaseSyncService ?? throw new ArgumentNullException(nameof(ISqlResourceDatabaseSyncService));
		}

		[FunctionName("SqlResourceDatabaseSync")]
		public async Task Run(
			[TimerTrigger("%SqlResourceDatabaseSyncSchedule%")] TimerInfo timerInfo,
			[Table("InventorySqlResourceDatabases", Connection = "StorageConnectionAppSetting")] CloudTable sqlResourceDatabaseTable,
			ILogger log)
		{
			log.LogInformation($"C# Sql Resource Database Sync Timer trigger function executed at: {DateTime.UtcNow}. Next occurrence: {timerInfo.FormatNextOccurrences(1)}");

			DataTable stageDataTable = new DataTable
			{
				Columns = { { "ID", typeof(int) }, { "ServerNameId", typeof(string) }, { "Name", typeof(string) }, { "ServerName", typeof(string) }, { "SubscriptionId", typeof(string) }, { "CreatedOn", typeof(DateTime) }, { "LastSeenOn", typeof(DateTime) } }
			};
			stageDataTable.PrimaryKey = new[] { stageDataTable.Columns[0] };

			TableContinuationToken token = null;
			do
			{
				TableQuerySegment<SqlServerDatabaseEntity> queryResult = await sqlResourceDatabaseTable.ExecuteQuerySegmentedAsync(new TableQuery<SqlServerDatabaseEntity>(), token);
				int index = 1;

				foreach (SqlServerDatabaseEntity result in queryResult)
				{
					DataRow row = stageDataTable.NewRow();
					row["ID"] = index;
					row["ServerNameId"] = result.ServerNameId;
					row["Name"] = result.RowKey;
					row["ServerName"] = result.PartitionKey;
					row["SubscriptionId"] = result.SubscriptionId;
					row["CreatedOn"] = result.CreatedOn;
					row["LastSeenOn"] = result.LastSeenOn;

					stageDataTable.Rows.Add(row);
					index++;
				}

				token = queryResult.ContinuationToken;
			} while (token != null);

			await _sqlResourceDatabaseSyncService.SyncSqlResourceDatabasesToAzSQLAsync(stageDataTable);
		}
	}
}
