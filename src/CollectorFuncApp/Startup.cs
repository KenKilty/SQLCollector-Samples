using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlCollector.Configurations;
using SqlCollector.Services;

[assembly: FunctionsStartup(typeof(SqlCollector.Startup))]

namespace SqlCollector
{
	// Register services see: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#register-services
	// IConfiguration see: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#working-with-options-and-settings
	// IOptions see: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			// Register SubscriptionInventoryService as scoped.
            // The same instance will be returned within the scope of a function invocation.
			builder
				.Services
				.AddScoped<ISubscriptionInventoryService, SubscriptionInventoryService>()
				.AddOptions<SubscriptionInventoryConfiguration>()
				.Configure<IConfiguration>((subscriptionInventorySettings, configuration) =>
				{
					configuration
					.GetSection("SubscriptionInventoryConfiguration")
					.Bind(subscriptionInventorySettings);
				})
				.Services
				.AddScoped<ISubscriptionSyncService, SubscriptionSyncService>()
				.AddOptions<SubscriptionSyncConfiguration>()
				.Configure<IConfiguration>((subscriptionSyncSettings, configuration) =>
				{
					configuration
					.GetSection("SubscriptionSyncConfiguration")
					.Bind(subscriptionSyncSettings);
				})
				.Services
				.AddScoped<ISqlResourceInventoryService, SqlResourceInventoryService>()
				.AddOptions<SqlResourceInventoryConfiguration>()
				.Configure<IConfiguration>((sqlResourceInventorySettings, configuration) =>
				{
					configuration
					.GetSection("SqlResourceInventoryConfiguration")
					.Bind(sqlResourceInventorySettings);
				})
				.Services
				.AddScoped<ISqlResourceInfoInventoryService, SqlResourceInfoInventoryService>()
				.AddOptions<SqlResourceInfoInventoryConfiguration>()
				.Configure<IConfiguration>((sqlResourceInfoInventorySettings, configuration) =>
				{
					configuration
					.GetSection("SqlResourceInfoInventoryConfiguration")
					.Bind(sqlResourceInfoInventorySettings);
				})
				.Services
				.AddScoped<ISqlResourceSyncService, SqlResourceSyncService>()
				.AddOptions<SqlResourceSyncConfiguration>()
				.Configure<IConfiguration>((sqlResourceSyncSettings, configuration) =>
				{
					configuration
					.GetSection("SqlResourceSyncConfiguration")
					.Bind(sqlResourceSyncSettings);
				});
		}
	}
}
