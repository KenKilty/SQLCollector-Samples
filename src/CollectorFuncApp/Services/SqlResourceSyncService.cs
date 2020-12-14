using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SqlCollector.Configurations;

namespace SqlCollector.Services
{
	public class SqlResourceSyncService : ISqlResourceSyncService
	{
		private readonly SqlResourceSyncConfiguration _sqlResourceSyncConfiguration;

		public SqlResourceSyncService(IOptions<SqlResourceSyncConfiguration> sqlResourceSyncConfiguration)
		{
			_sqlResourceSyncConfiguration = sqlResourceSyncConfiguration.Value;
		}

		public Task SyncSqlResourcesToAzSQL(DataTable stageDataTable)
		{
			SyncSqlResourcesToAzSQL(stageDataTable).RunSynchronously();
			return Task.CompletedTask;
		}

		public async Task SyncSqlResourcesToAzSQLAsync(DataTable stageDataTable)
		{
			using SqlConnection destinationConnection = new SqlConnection(_sqlResourceSyncConfiguration.SQLCollectorDbConnectionString);
			await destinationConnection.OpenAsync();
			using SqlCommand truncateStageCommand = new SqlCommand("TRUNCATE TABLE [app].[SqlResourceStage]", destinationConnection);
			using SqlCommand upsertStageCommand = new SqlCommand("EXECUTE [app].[uspSqlResourceStageUpsert]", destinationConnection);
			using SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection)
			{
				DestinationTableName = "[app].[SqlResourceStage]"
			};
			_ = await truncateStageCommand.ExecuteNonQueryAsync();
			await bulkCopy.WriteToServerAsync(stageDataTable);
			_ = await upsertStageCommand.ExecuteNonQueryAsync();
		}
	}
}
