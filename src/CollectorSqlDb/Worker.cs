using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SqlCollectorDb.Data;

namespace SqlCollectorDb
{
	public class WorkerService : BackgroundService
	{
		private readonly ILogger<WorkerService> _logger;
		private readonly IHostApplicationLifetime _lifetime;
		private readonly IServiceScopeFactory  _scopeFactory;

		public WorkerService(ILogger<WorkerService> logger, IHostApplicationLifetime lifetime, IServiceScopeFactory  scopeFactory)
		{
			_logger = logger;
			_lifetime = lifetime;
			_scopeFactory = scopeFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

					using (var scope = _scopeFactory.CreateScope())
					{
						SqlCollectorDbContext dbContext = scope.ServiceProvider.GetRequiredService<SqlCollectorDbContext>();
						_ = await dbContext.Database.ExecuteSqlRawAsync("SELECT @@VERSION");
					}

					await Task.Delay(1000, stoppingToken);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured. Stopping the WorkerService gracefullly.");
				_lifetime.StopApplication();
				throw;
			}
		}
	}
}
