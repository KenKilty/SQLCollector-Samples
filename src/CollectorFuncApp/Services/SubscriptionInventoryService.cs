using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using SqlCollector.Configurations;
using SqlCollector.Models;
using SqlCollector.Tools;

namespace SqlCollector.Services
{
	public class SubscriptionInventoryService : ISubscriptionInventoryService
	{
		private readonly SubscriptionInventoryConfiguration _subscriptionInventoryConfiguration;

		public SubscriptionInventoryService(IOptions<SubscriptionInventoryConfiguration> subscriptionInventoryConfiguration)
		{
			_subscriptionInventoryConfiguration = subscriptionInventoryConfiguration.Value;
		}

		public IReadOnlyList<SubscriptionDto> GetSubscriptions()
		{
			return GetSubscriptionsAsync().Result;
		}

		public async Task<IReadOnlyList<SubscriptionDto>> GetSubscriptionsAsync()
		{
			DefaultAzureCredential credential;
			if (_subscriptionInventoryConfiguration.UseSystemAssignedManagedIdentity) {
				credential = new DefaultAzureCredential();
			}
			else {
				credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = _subscriptionInventoryConfiguration.UserAssignedManagedIdentityClientId });
			}

			TokenRequestContext tokenRequestContext = new TokenRequestContext(new[] { Constants.AzureResourceManagerAPIDefaultScope });
			AccessToken tokenRequestResult = await credential.GetTokenAsync(tokenRequestContext);
			ServiceClientCredentials serviceClientCreds = new TokenCredentials(tokenRequestResult.Token);

			SubscriptionClient subClient = new SubscriptionClient(serviceClientCreds);

			IPage<Subscription> subs = await subClient.Subscriptions.ListAsync();
			IReadOnlyList<SubscriptionDto> subscriptions = subs?.ToList()?.Select(s => new SubscriptionDto() { SubscriptionId = s.SubscriptionId, SubscriptionName = s.DisplayName }).ToList().AsReadOnly();

			return subscriptions;
		}
	}
}
