﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SqlCollectorDb.Data;

namespace SqlCollectorDb.Migrations
{
	[DbContext(typeof(SqlCollectorDbContext))]
	partial class SqlCollectorDbContextModelSnapshot : ModelSnapshot
	{
		protected override void BuildModel(ModelBuilder modelBuilder)
		{
#pragma warning disable 612, 618
			modelBuilder
				.UseIdentityColumns()
				.HasAnnotation("Relational:MaxIdentifierLength", 128)
				.HasAnnotation("ProductVersion", "5.0.0");

			modelBuilder.Entity("SqlCollectorDb.Models.Subscription", b =>
				{
					b.Property<Guid>("Id")
						.HasColumnType("uniqueidentifier")
						.HasColumnName("ID");

					b.Property<DateTime>("CreatedOn")
						.HasColumnType("datetime2");

					b.Property<DateTime>("LastSeenOn")
						.HasColumnType("datetime2");

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(255)
						.IsUnicode(false)
						.HasColumnType("varchar(255)");

					b.HasKey("Id");

					b.ToTable("Subscription", "app");
				});

			modelBuilder.Entity("SqlCollectorDb.Models.SubscriptionHistory", b =>
				{
					b.Property<int>("HistoryId")
						.ValueGeneratedOnAdd()
						.HasColumnType("int")
						.HasColumnName("HistoryID")
						.UseIdentityColumn();

					b.Property<DateTime>("ArchivedOn")
						.ValueGeneratedOnAdd()
						.HasColumnType("datetime2")
						.HasDefaultValueSql("(sysutcdatetime())");

					b.Property<DateTime>("CreatedOn")
						.HasColumnType("datetime2");

					b.Property<Guid>("Id")
						.HasColumnType("uniqueidentifier")
						.HasColumnName("ID");

					b.Property<DateTime>("LastSeenOn")
						.HasColumnType("datetime2");

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(255)
						.IsUnicode(false)
						.HasColumnType("varchar(255)");

					b.HasKey("HistoryId")
						.HasName("PK_Subscription");

					b.ToTable("SubscriptionHistory", "history");
				});

			modelBuilder.Entity("SqlCollectorDb.Models.SubscriptionStage", b =>
				{
					b.Property<Guid>("Id")
						.HasColumnType("uniqueidentifier")
						.HasColumnName("ID");

					b.Property<DateTime>("CreatedOn")
						.HasColumnType("datetime2");

					b.Property<DateTime>("LastSeenOn")
						.HasColumnType("datetime2");

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(255)
						.IsUnicode(false)
						.HasColumnType("varchar(255)");

					b.HasKey("Id");

					b.ToTable("SubscriptionStage", "app");
				});
#pragma warning restore 612, 618
		}
	}
}
