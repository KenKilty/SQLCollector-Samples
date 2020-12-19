using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SqlCollector.Configurations;

namespace SqlCollector.Services
{
	public class SqlResourceDatabaseSyncService : ISqlResourceDatabaseSyncService
	{
		private readonly SqlResourceDatabaseSyncConfiguration _sqlResourceDatabaseSyncConfiguration;

		public SqlResourceDatabaseSyncService(IOptions<SqlResourceDatabaseSyncConfiguration> sqlResourceDatabaseSyncConfiguration)
		{
			_sqlResourceDatabaseSyncConfiguration = sqlResourceDatabaseSyncConfiguration.Value;
		}

		public Task SyncSqlResourceDatabasesToAzSQL(DataTable stageDataTable)
		{
			SyncSqlResourceDatabasesToAzSQLAsync(stageDataTable).RunSynchronously();
			return Task.CompletedTask;
		}

		public async Task SyncSqlResourceDatabasesToAzSQLAsync(DataTable stageDataTable)
		{
			using SqlConnection destinationConnection = new SqlConnection(_sqlResourceDatabaseSyncConfiguration.SQLCollectorDbConnectionString);
			await destinationConnection.OpenAsync();
			using SqlCommand truncateStageCommand = new SqlCommand("TRUNCATE TABLE [app].[SqlResourceDatabaseStage]", destinationConnection);
			using SqlCommand upsertStageCommand = new SqlCommand("EXECUTE [app].[uspSqlResourceDatabaseStageUpsert]", destinationConnection);
			using SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection)
			{
				DestinationTableName = "[app].[SqlResourceDatabaseStage]"
			};
			_ = await truncateStageCommand.ExecuteNonQueryAsync();
			await bulkCopy.WriteToServerAsync(stageDataTable);
			_ = await upsertStageCommand.ExecuteNonQueryAsync();
		}
	}
}
