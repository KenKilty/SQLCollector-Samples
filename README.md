# SqlCollector-Samples

Sample fictious application using Azure Functions 3.x to illustrate serverless data collection targeting Azure SQL and SQL Server on Virtual Machines.
Illustrates usage of [DefaultAzureCredential](https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) in conjunction with Azure Active Directory Managed Identity Authentication for accessing Azure SQL. This is made possible using the enhancements to the dotnet Microsoft.Data.SqlClient 2.1 enhancements supporting managed identity with zero code changes related to token aquisition.

## Inventory SQL Database

The SQL Collector Database schema is maintained as a EF Core 5 code first dotnet console project. The schema is simple consisting of tables for subscription and resource inventory related to SQL assets running in Azure along supporting schema and stored procedures.

The managed identity used by the serverless Azure Functions compute host require a [contained database user](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-user-transact-sql?view=sql-server-ver15#j-create-an-azure-ad-user-without-an-aad-login-for-the-database) created withing the SqlCollectorDb database with appropriate write permissions in the database.

```SQL
CREATE USER [<ManagedIdentityDisplayName>] FROM EXTERNAL PROVIDER;
GO

<assign role or object level permissions for this user>
```

Deployment of the database is managed within an Azure DevOps pipeline using idempotent migration T-SQL script generation.

```migrations script --project $(workingDirectory)/CollectorSqlDb/SqlCollectorDb.csproj --configuration ${{ parameters.buildConfiguration }} --idempotent --output $(System.DefaultWorkingDirectory)/CollectorSqlDb/publish_output/migrations.sql```

The migration script is then deployed to the Azure SQL database using managed the [managed identity of the pipeline build agent](https://docs.microsoft.com/en-us/azure/devops/pipelines/library/connect-to-azure?view=azure-devops#create-an-azure-resource-manager-service-connection-to-a-vm-with-a-managed-service-identity) by first requsting a token for the database.windows.net resource and subsequent execution by using the SqlServer PowerShell module using a Azure PowerShell task in the Azure DevOps pipeline.

```Powershell
$access_token = (Get-AzAccessToken -ResourceUrl https://database.windows.net).Token
Invoke-Sqlcmd -InputFile "$(System.DefaultWorkingDirectory)/CollectorSqlDb/publish_output/migrations.sql" -ServerInstance $env:targetSqlCollectorDb -Database SqlCollectorDb -AccessToken $access_token
```

## SQL Collector - Subscription Architecture

![Subscription Inventory Flow](resources\subsinventory.png)

A single user assigned managed identity is used for the following:

- Azure DevOps self-hosted build agent on Azure VM
  - Access to the subscription hosting the resources
  - Access to the Azure SQL database for deploying migrations
- Azure Functions Premium Host w/VNET
  - Access to query the ARM API for subscription discovery
  - Acesss to query the Azure Resource Graph for inventory data
- Azure SQL Database contained database user
  - Access to write to the inventory tables

For seperation of duties it would be fine to break out each identity if necessary. System assigned identies for Azure DevOps build agent and Azure Functions Host are an option as well.

## SQL Collector - Subscription Inventory

Azure subscription data is collected by the SubscriptionInventory Azure function and persisted to an Azure Table using the Cosmos Db Table API NuGet package Microsoft.Azure.Cosmos.Table. The Azure Table acts as a buffer between the data collected and Azure SQL downstream. A possible enhancement would be to replace the Table endpoint with Cosmos Db and use the Change Data Capture to broadcast subscription discovery.

At present managed identity authentication via Azure Function binding is not support so a Azure Table binding is used. Storage for the Azure Table is seperate from the Azure WebJobs Storage for the function but this optional.

```C#
[FunctionName("SubscriptionInventory")]
  public async Task Run(
  [TimerTrigger("%SubscriptionInventorySchedule%")] TimerInfo timerInfo,
  [Table("InventorySubscription", Connection = "StorageConnectionAppSetting")] CloudTable inventorySubscription,
  ILogger log)
```

The managed identity of the Azure Functions compute host is used along with the dotnet SDK to query for Azure Subscriptions that the managed identity has reader permissions to view. Note the differentiation between system assign and user assigned managed identity which is defined in the IOptions configuration for the function.

```C#
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
```

## SQL Collector - Sync to Azure SQL

Azure subscription inventory data is loaded into a staging table from the SubscriptionSync Azure Function into the table 'SubscriptionStage' and merged with 'Subscription' and 'SubscriptionHistory' for delta storage using an upsert operation in T-SQL.

```SQL
INSERT INTO [history].[SubscriptionHistory]
SELECT [ID],
       [Name],
       [CreatedOn],
       [LastSeenOn],
       [ArchivedOn]
FROM
  (MERGE [app].[Subscription] AS T
   USING [app].[SubscriptionStage] AS S ON T.[ID] = S.[ID]
   WHEN NOT MATCHED BY TARGET THEN
     INSERT([ID], [Name], [CreatedOn], [LastSeenOn])
     VALUES (S.[ID],
           S.[Name],
           S.[CreatedOn],
           S.[LastSeenOn])
   WHEN MATCHED THEN
     UPDATE
     SET T.[ID] = S.[ID],
       T.[Name] = S.[Name],
       T.[CreatedOn] = S.[CreatedOn],
       T.[LastSeenOn] = SYSUTCDATETIME()
   OUTPUT $action,inserted.*,SYSUTCDATETIME() AS ArchivedOn) AS[Changes]([Action], [ID], [Name], [CreatedOn], [LastSeenOn], [ArchivedOn])
   WHERE [Action] = 'UPDATE'
```
The subscription employing [user assigned managed identity](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview#managed-identity-types) with [Microsoft.Data.SqlClient v2.1](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) to managed token aquisition:

`Server=tcp:sqlcollector-contoso.database.windows.net,1433; Authentication=Active Directory Managed Identity; User Id=d3004180-edf4-4a9c-8fb6-19036d8c620b; Initial Catalog=SqlCollectorDb;`

## SQL Collector - Resource Inventory Architecture

To be completed.

## Reference

- [Azure Active Directory Managed Identity authentication for Azure SQL](https://github.com/dotnet/SqlClient/blob/master/release-notes/2.1/2.1.0.md#azure-active-directory-managed-identity-authentication)

## License

Copyright 2020 Kenneth Kilty

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
