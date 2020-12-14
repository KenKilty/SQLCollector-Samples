using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Newtonsoft.Json.Linq;
using SqlCollector.Configurations;
using SqlCollector.Models;
using SqlCollector.Tools;

namespace SqlCollector.Services
{
	public class SqlResourceInventoryService : ISqlResourceInventoryService
	{
		private readonly SqlResourceInventoryConfiguration _sqlResourceInventoryConfiguration;

		public SqlResourceInventoryService(IOptions<SqlResourceInventoryConfiguration> sqlResourceInventoryConfiguration)
		{
			_sqlResourceInventoryConfiguration = sqlResourceInventoryConfiguration.Value;
		}

		public IReadOnlyList<ResourceDto> GetSqlResources(string _subscriptionId)
		{
			return GetSqlResourcesAsync(_subscriptionId).Result;
		}

		public async Task<IReadOnlyList<ResourceDto>> GetSqlResourcesAsync(string _subscriptionId)
		{
			if (string.IsNullOrWhiteSpace(_subscriptionId))
			{
				return null;
			}

			DefaultAzureCredential credential;
			if (_sqlResourceInventoryConfiguration.UseSystemAssignedManagedIdentity) {
				credential = new DefaultAzureCredential();
			}
			else {
				credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = _sqlResourceInventoryConfiguration.UserAssignedManagedIdentityClientId });
			}

			TokenRequestContext tokenRequestContext = new TokenRequestContext(new[] { Constants.AzureResourceManagerAPIDefaultScope });
			AccessToken tokenRequestResult = await credential.GetTokenAsync(tokenRequestContext);
			ServiceClientCredentials serviceClientCreds = new TokenCredentials(tokenRequestResult.Token);

			ResourceGraphClient argClient = new ResourceGraphClient(serviceClientCreds);
			QueryRequest request = new QueryRequest();
			request.Subscriptions = new List<string>() { _subscriptionId };
			request.Query = "Resources |" +
				" where type in ('microsoft.sql/servers','microsoft.sqlvirtualmachine/sqlvirtualmachines','Microsoft.Sql/managedInstances') |" +
				" project id, name, type, location, resourceGroup, subscriptionId, tenantId";
			request.Options = new QueryRequestOptions() { ResultFormat = ResultFormat.ObjectArray };

			// Parameter to hold full list of returned resources
			List<ResourceDto> results = new List<ResourceDto>();


			// Send query to the ResourceGraphClient and get response
			QueryResponse response = argClient.Resources(request);


			// IMPORTANT: The query must project the id field in order for pagination to work.
			// If it's missing from the query, the response won't include the $skipToken.
			if (response.Count > 0)
			{
				// Add response results to list
				results.AddRange(((JArray)response.Data).ToObject<List<ResourceDto>>());

				// Continue till SkipToken is null
				while (!string.IsNullOrWhiteSpace(response.SkipToken))
				{
					// Update request with new skip token returned from response
					request.Options.SkipToken = response.SkipToken;

					// Send query with SkipToken to the ResourceGraphClient and get response
					response = argClient.Resources(request);

					// Add response results to list
					results.AddRange(((JArray)response.Data).ToObject<List<ResourceDto>>());
				}
			}

			IReadOnlyList<ResourceDto> returnList = results.ToList().AsReadOnly();


			return returnList;
		}
	}
}
