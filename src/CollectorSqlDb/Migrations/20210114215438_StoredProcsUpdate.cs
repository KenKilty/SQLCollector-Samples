using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlCollectorDb.Migrations
{
    public partial class StoredProcsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			string uspSqlResourceStageUpsert = @"
					DECLARE @uspSqlResourceStageUpsert NVARCHAR(MAX) = N'
					CREATE PROCEDURE [app].[uspSqlResourceStageUpsert] AS
					INSERT INTO [history].[SqlResourceHistory]
					SELECT [ID], [ResourceId], [Name], [AdminLogin], [Type], [CreatedOn], [LastSeenOn], [ArchivedOn]
					FROM (
						MERGE [app].[SqlResource] AS T
						USING [app].[SqlResourceStage] AS S
						ON T.[ID] = S.[ID]
						WHEN NOT MATCHED BY TARGET
							THEN INSERT([ResourceId],[Name],[AdminLogin],[Type],[CreatedOn],[LastSeenOn]) VALUES (S.[ResourceId],S.[Name],S.[AdminLogin],S.[Type],S.[CreatedOn],S.[LastSeenOn])
						WHEN MATCHED
							THEN UPDATE SET T.[ResourceId] = S.[ResourceId],
									T.[Name] = S.[Name],
									T.[AdminLogin] = S.[AdminLogin],
									T.[Type] = S.[Type],
									T.[CreatedOn] = S.[CreatedOn],
									T.[LastSeenOn] = SYSUTCDATETIME()
						OUTPUT $action, inserted.*, SYSUTCDATETIME() AS ArchivedOn) AS [Changes]([Action], [ID], [ResourceId], [Name], [AdminLogin], [Type], [CreatedOn], [LastSeenOn], [ArchivedOn])
					WHERE [Action] = ''UPDATE'''
					EXEC sp_executesql @uspSqlResourceStageUpsert";

			migrationBuilder.Sql(uspSqlResourceStageUpsert);

			string uspSqlResourceDatabaseStageUpsert = @"
					DECLARE @uspSqlResourceDatabaseStageUpsert NVARCHAR(MAX) = N'
					CREATE PROCEDURE [app].[uspSqlResourceDatabaseStageUpsert] AS
					INSERT INTO [history].[SqlResourceDatabaseHistory]
					SELECT [ID], [ServerNameId], [Name], [ServerName], [SubscriptionId], [CreatedOn], [LastSeenOn], [ArchivedOn]
					FROM (
						MERGE [app].[SqlResourceDatabase] AS T
						USING [app].[SqlResourceDatabaseStage] AS S
						ON T.[ID] = S.[ID]
						WHEN NOT MATCHED BY TARGET
							THEN INSERT([ServerNameId],[Name],[ServerName],[SubscriptionId],[CreatedOn],[LastSeenOn]) VALUES (S.[ServerNameId],S.[Name],S.[ServerName],S.[SubscriptionId],S.[CreatedOn],S.[LastSeenOn])
						WHEN MATCHED
							THEN UPDATE SET T.[ServerNameId] = S.[ServerNameId],
									T.[Name] = S.[Name],
									T.[ServerName] = S.[ServerName],
									T.[SubscriptionId] = S.[SubscriptionId],
									T.[CreatedOn] = S.[CreatedOn],
									T.[LastSeenOn] = SYSUTCDATETIME()
						OUTPUT $action, inserted.*, SYSUTCDATETIME() AS ArchivedOn) AS [Changes]([Action], [ID], [ServerNameId], [Name], [ServerName], [SubscriptionId], [CreatedOn], [LastSeenOn], [ArchivedOn])
					WHERE [Action] = ''UPDATE'''
					EXEC sp_executesql @uspSqlResourceDatabaseStageUpsert";

			migrationBuilder.Sql(uspSqlResourceDatabaseStageUpsert);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE [app].[uspSqlResourceStageUpsert]");

			migrationBuilder.Sql("DROP PROCEDURE [app].[uspSqlResourceDatabaseStageUpsert]");
		}
    }
}
