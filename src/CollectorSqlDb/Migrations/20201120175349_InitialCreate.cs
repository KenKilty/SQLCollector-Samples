using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlCollectorDb.Migrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
				name: "app");

			migrationBuilder.EnsureSchema(
				name: "history");

			migrationBuilder.CreateTable(
				name: "Subscription",
				schema: "app",
				columns: table => new
				{
					ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Subscription", x => x.ID);
				});

			migrationBuilder.CreateTable(
				name: "SubscriptionHistory",
				schema: "history",
				columns: table => new
				{
					HistoryID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					ArchivedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Subscription", x => x.HistoryID);
				});

			migrationBuilder.CreateTable(
				name: "SubscriptionStage",
				schema: "app",
				columns: table => new
				{
					ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SubscriptionStage", x => x.ID);
				});

			string uspSubscriptionStageUpsert = @"
					DECLARE @uspSubscriptionStageUpsert NVARCHAR(MAX) = N'
					CREATE PROCEDURE [app].[uspSubscriptionStageUpsert] AS
					INSERT INTO [history].[SubscriptionHistory]
					SELECT [ID], [Name], [CreatedOn], [LastSeenOn], [ArchivedOn]
					FROM (
						MERGE [app].[Subscription] AS T
						USING [app].[SubscriptionStage] AS S
						ON T.[ID] = S.[ID]
						WHEN NOT MATCHED BY TARGET
							THEN INSERT([ID],[Name],[CreatedOn],[LastSeenOn]) VALUES (S.[ID],S.[Name],S.[CreatedOn],S.[LastSeenOn])
						WHEN MATCHED
							THEN UPDATE SET T.[ID] = S.[ID],
									T.[Name] = S.[Name],
									T.[CreatedOn] = S.[CreatedOn],
									T.[LastSeenOn] = SYSUTCDATETIME()
						OUTPUT $action, inserted.*, SYSUTCDATETIME() AS ArchivedOn) AS [Changes]([Action], [ID], [Name], [CreatedOn], [LastSeenOn], [ArchivedOn])
					WHERE [Action] = ''UPDATE'''
					EXEC sp_executesql @uspSubscriptionStageUpsert";

			migrationBuilder.Sql(uspSubscriptionStageUpsert);

			
			migrationBuilder.CreateTable(
				name: "SqlResource",
				schema: "app",
				columns: table => new
				{
					ID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ResourceId = table.Column<Guid>(type: "varchar(255)", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					AdminLogin = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
					Type = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SqlResource", x => x.ID);
				});

			migrationBuilder.CreateTable(
				name: "SqlResourceHistory",
				schema: "history",
				columns: table => new
				{
					HistoryID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ID = table.Column<Guid>(type: "int", nullable: false),
					ResourceId = table.Column<Guid>(type: "varchar(255)", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					AdminLogin = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
					Type = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: true),
					ArchivedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SqlResource", x => x.HistoryID);
				});

			migrationBuilder.CreateTable(
				name: "SqlResourceStage",
				schema: "app",
				columns: table => new
				{
					ID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ResourceId = table.Column<Guid>(type: "varchar(255)", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					AdminLogin = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
					Type = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SqlResourceStage", x => x.ID);
				});

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




			migrationBuilder.CreateTable(
				name: "SqlResourceDatabase",
				schema: "app",
				columns: table => new
				{
					ID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ServerNameId = table.Column<Guid>(type: "varchar(255)", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					ServerName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
					SubscriptionId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SqlResourceDatabase", x => x.ID);
				});

			migrationBuilder.CreateTable(
				name: "SqlResourceDatabaseHistory",
				schema: "history",
				columns: table => new
				{
					HistoryID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ID = table.Column<Guid>(type: "int", nullable: false),
					ServerNameId = table.Column<Guid>(type: "varchar(255)", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					ServerName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
					SubscriptionId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: true),
					ArchivedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SqlResourceDatabase", x => x.HistoryID);
				});

			migrationBuilder.CreateTable(
				name: "SqlResourceDatabaseStage",
				schema: "app",
				columns: table => new
				{
					ID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ServerNameId = table.Column<Guid>(type: "varchar(255)", nullable: false),
					Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					ServerName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
					SubscriptionId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
					CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastSeenOn = table.Column<DateTime>(type: "datetime2", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SqlResourceDatabaseStage", x => x.ID);
				});

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
			migrationBuilder.DropTable(
				name: "Subscription",
				schema: "app");

			migrationBuilder.DropTable(
				name: "SubscriptionHistory",
				schema: "history");

			migrationBuilder.DropTable(
				name: "SubscriptionStage",
				schema: "app");

			migrationBuilder.Sql("DROP PROCEDURE [app].[uspSubscriptionStageUpsert]");

			migrationBuilder.DropTable(
				name: "SqlResource",
				schema: "app");

			migrationBuilder.DropTable(
				name: "SqlResourceHistory",
				schema: "history");

			migrationBuilder.DropTable(
				name: "SqlResourceStage",
				schema: "app");

			migrationBuilder.Sql("DROP PROCEDURE [app].[uspSqlResourceStageUpsert]");

			migrationBuilder.DropTable(
				name: "SqlResourceDatabase",
				schema: "app");

			migrationBuilder.DropTable(
				name: "SqlResourceDatabaseHistory",
				schema: "history");

			migrationBuilder.DropTable(
				name: "SqlResourceDatabaseStage",
				schema: "app");

			migrationBuilder.Sql("DROP PROCEDURE [app].[uspSqlResourceDatabaseStageUpsert]");
		}
	}
}
