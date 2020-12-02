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
		}
	}
}
