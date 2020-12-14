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
    public class SqlResourceCollector
    {
		private readonly ISqlResourceInventoryService _sqlResourceInventoryService;

		public SqlResourceCollector(ISqlResourceInventoryService sqlResourceInventoryService)
		{
			_sqlResourceInventoryService = sqlResourceInventoryService ?? throw new ArgumentNullException(nameof(ISqlResourceInventoryService));
		}

		[FunctionName("SqlResourceCollector")]
        public async Task Run
		([QueueTrigger("outqueue")] string input,
		[Queue("outsqlqueue"), StorageAccount("AzureWebJobsStorage")] ICollector<ResourceDto> msg,
		ILogger log)
        {
			log.LogInformation($"C# Sql Resource Collector function executed at: {DateTime.UtcNow}.");

			IReadOnlyList<ResourceDto> resources = await _sqlResourceInventoryService.GetSqlResourcesAsync(input);

			foreach (var res in resources)
			{
				msg.Add(res);
			}
		}
    }
}
