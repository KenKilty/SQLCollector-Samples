using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Newtonsoft.Json.Linq;
using SqlCollector.Configurations;
using SqlCollector.Models;
using SqlCollector.Tools;

namespace SqlCollector.Services
{
	public class SqlResourceInfoInventoryService : ISqlResourceInfoInventoryService
	{
		private readonly SqlResourceInfoInventoryConfiguration _sqlResourceInfoInventoryConfiguration;

		public SqlResourceInfoInventoryService(IOptions<SqlResourceInfoInventoryConfiguration> sqlResourceInfoInventoryConfiguration)
		{
			_sqlResourceInfoInventoryConfiguration = sqlResourceInfoInventoryConfiguration.Value;
		}

		public SqlResourceDto GetSqlServerResource(ResourceDto _sqlResource)
		{
			return GetSqlServerResourceAsync(_sqlResource).Result;
		}

		public async Task<SqlResourceDto> GetSqlServerResourceAsync(ResourceDto _sqlResource)
		{
			if (_sqlResource == null || _sqlResource == default(ResourceDto))
			{
				return null;
			}

			DefaultAzureCredential credential;
			if (_sqlResourceInfoInventoryConfiguration.UseSystemAssignedManagedIdentity)
			{
				credential = new DefaultAzureCredential();
			}
			else
			{
				credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = _sqlResourceInfoInventoryConfiguration.UserAssignedManagedIdentityClientId });
			}

			TokenRequestContext tokenRequestContextGraph = new TokenRequestContext(new[] { Constants.AzureResourceGraphAPIDefaultScope });
			AccessToken tokenRequestResultGraph = await credential.GetTokenAsync(tokenRequestContextGraph);
			TokenRequestContext tokenRequestContextARM = new TokenRequestContext(new[] { Constants.AzureResourceManagerAPIDefaultScope });
			AccessToken tokenRequestResultARM = await credential.GetTokenAsync(tokenRequestContextARM);


			// Credentials used for authenticating a fluent management client to Azure.
			AzureCredentials credentials = new AzureCredentials(
								new TokenCredentials(tokenRequestResultARM.Token),
								new TokenCredentials(tokenRequestResultGraph.Token),
								_sqlResource.tenantId,
								AzureEnvironment.AzureGlobalCloud);

			// Top level abstraction of Azure. https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.management.fluent.iazure?view=azure-dotnet
			// .WithSubscription is optional if you wish to return resource beyond the scope of a single subscription.
			IAzure azure = Microsoft.Azure.Management.Fluent.Azure
							.Configure()
							.WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
							.Authenticate(credentials)
							.WithSubscription(_sqlResource.subscriptionId);


			// Iterate through Microsoft.Sql top level resources (servers) and a list of databases (sub resources)
			// for data collection define IList<T> outside of these nested loops and add resources and sub resources
			// of interest to collections.
			ISqlServer server = await azure.SqlServers.GetByIdAsync(_sqlResource.id);

			if (server != null)
			{
				SqlResourceDto newServer = new SqlResourceDto()
				{
					ServerNameId = server.Id,
					ServerName = server.Name,
					SubscriptionId = _sqlResource.subscriptionId,
					AdminLogin = server.AdministratorLogin
				};

				IReadOnlyList<ISqlDatabase> databasess = await server.Databases.ListAsync();
				foreach (ISqlDatabase database in databasess)
				{
					SqlServerDatabaseDto db = new SqlServerDatabaseDto()
					{
						ServerNameId = server.Id,
						DatabaseName = database.Name,
						ServerName = server.Name,
						CreatedOn = database.CreationDate
					};

					newServer.Databases.Add(db);
				}

				return newServer;
			}

			return null;
		}
	}
}
