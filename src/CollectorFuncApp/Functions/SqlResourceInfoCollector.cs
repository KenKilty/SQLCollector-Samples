using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SqlCollector.Models;
using SqlCollector.Services;

namespace CollectorFuncApp.Functions
{
    public class SqlResourceInfoCollector
    {
		private readonly ISqlResourceInfoInventoryService _sqlResourceInfoInventoryService;

		public SqlResourceInfoCollector(ISqlResourceInfoInventoryService sqlResourceInfoInventoryService)
		{
			_sqlResourceInfoInventoryService = sqlResourceInfoInventoryService ?? throw new ArgumentNullException(nameof(ISqlResourceInfoInventoryService));
		}

		[Singleton]
		[FunctionName("SqlResourceInfoCollector")]
        public async Task Run
		([QueueTrigger("outsqlqueue")] ResourceDto input,
		[Table("InventorySqlResource", Connection = "StorageConnectionAppSetting")] CloudTable sqlInfoTable,
		[Table("InventorySqlResourceDatabases", Connection = "StorageConnectionAppSetting")] CloudTable sqlDatabasesTable,
		ILogger log)
        {
			log.LogInformation($"C# Sql Resource Info Collector queue trigger function executed at: {DateTime.UtcNow}.");


			if (input.type == "microsoft.sql/servers")
			//TO DO: managed instance and vm types
			//if (input.type == "microsoft.sqlvirtualmachine/sqlvirtualmachines")
			{
				var sqlresource = await _sqlResourceInfoInventoryService.GetSqlServerResourceAsync(input);

				if (sqlresource != null)
				{
					// add in sql resource into table
					await sqlInfoTable.CreateIfNotExistsAsync();
					DateTime init = DateTime.Now;

					SqlResourceEntity sqlEntity = new SqlResourceEntity()
					{
						PartitionKey = sqlresource.SubscriptionId,
						RowKey = sqlresource.ServerName,
						ServerName = sqlresource.ServerNameId,
						AdminLogin = sqlresource.AdminLogin,
						CreatedOn = init,
						LastSeenOn = init
					};

					TableOperation retrieveOperation = TableOperation.Retrieve<SqlResourceEntity>(sqlresource.SubscriptionId, sqlresource.ServerName);
					TableResult retrievedEntity = await sqlInfoTable.ExecuteAsync(retrieveOperation);

					if (retrievedEntity.Result == null)
					{
						TableOperation insertOperation = TableOperation.Insert(sqlEntity);
						await sqlInfoTable.ExecuteAsync(insertOperation);
					}
					else
					{
						sqlEntity.CreatedOn = ((SqlResourceEntity)retrievedEntity.Result).CreatedOn;
						TableOperation mergeOperation = TableOperation.InsertOrMerge(sqlEntity);
						await sqlInfoTable.ExecuteAsync(mergeOperation);
					}

					// cycle through sql resource databases and insert them into a table
					if (sqlresource.Databases != null)
					{
						// add in sql resource into table
						await sqlDatabasesTable.CreateIfNotExistsAsync();


						foreach (SqlServerDatabaseDto db in sqlresource.Databases)
						{
							SqlServerDatabaseEntity subEntity = new SqlServerDatabaseEntity()
							{
								PartitionKey = db.ServerName,
								RowKey = db.DatabaseName,
								SubscriptionId = db.SubscriptionId,
								ServerName = db.ServerName,
								CreatedOn = init,
								LastSeenOn = init
							};

							TableOperation retrieveOperationDb = TableOperation.Retrieve<SqlServerDatabaseEntity>(db.ServerName, db.DatabaseName);
							TableResult retrievedEntityDb = await sqlDatabasesTable.ExecuteAsync(retrieveOperationDb);

							if (retrievedEntityDb.Result == null)
							{
								TableOperation insertOperation = TableOperation.Insert(subEntity);
								await sqlDatabasesTable.ExecuteAsync(insertOperation);
							}
							else
							{
								subEntity.CreatedOn = ((SqlServerDatabaseEntity)retrievedEntityDb.Result).CreatedOn;
								TableOperation mergeOperation = TableOperation.InsertOrMerge(subEntity);
								await sqlDatabasesTable.ExecuteAsync(mergeOperation);
							}
						}
					}
				}
			}
		}
    }
}
