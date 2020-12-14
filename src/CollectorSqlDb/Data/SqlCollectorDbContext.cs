using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SqlCollectorDb.Models;

#nullable disable

namespace SqlCollectorDb.Data
{
	public partial class SqlCollectorDbContext : DbContext
	{
		public SqlCollectorDbContext()
		{
		}

		public SqlCollectorDbContext(DbContextOptions<SqlCollectorDbContext> options)
			: base(options)
		{
		}

		public virtual DbSet<Subscription> Subscriptions { get; set; }
		public virtual DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }
		public virtual DbSet<SubscriptionStage> SubscriptionStages { get; set; }
		public virtual DbSet<SqlResource> SqlResources { get; set; }
		public virtual DbSet<SqlResourceHistory> SqlResourceHistories { get; set; }
		public virtual DbSet<SqlResourceStage> SqlResourceStages { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer("Name=SqlCollectorDb");
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Subscription>(entity =>
			{
				entity.ToTable("Subscription", "app");

				entity.Property(e => e.Id)
					.ValueGeneratedNever()
					.HasColumnName("ID");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);
			});

			modelBuilder.Entity<SubscriptionHistory>(entity =>
			{
				entity.HasKey(e => e.HistoryId)
					.HasName("PK_Subscription");

				entity.ToTable("SubscriptionHistory", "history");

				entity.Property(e => e.HistoryId).HasColumnName("HistoryID");

				entity.Property(e => e.ArchivedOn).HasDefaultValueSql("(sysutcdatetime())");

				entity.Property(e => e.Id).HasColumnName("ID");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);
			});

			modelBuilder.Entity<SubscriptionStage>(entity =>
			{
				entity.ToTable("SubscriptionStage", "app");

				entity.Property(e => e.Id)
					.ValueGeneratedNever()
					.HasColumnName("ID");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);
			});

			modelBuilder.Entity<SqlResource>(entity =>
			{
				entity.ToTable("SqlResource", "app");

				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).HasColumnName("ID");

				entity.Property(e => e.ResourceId)
					.ValueGeneratedNever()
					.HasColumnName("ResourceId");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.AdminLogin)
					.HasMaxLength(255)
					.IsUnicode(false);
			});

			modelBuilder.Entity<SqlResourceHistory>(entity =>
			{
				entity.HasKey(e => e.HistoryId)
					.HasName("PK_SqlResource");

				entity.ToTable("SqlResourceHistory", "history");

				entity.Property(e => e.HistoryId).HasColumnName("HistoryID");

				entity.Property(e => e.ArchivedOn).HasDefaultValueSql("(sysutcdatetime())");

				entity.Property(e => e.Id).HasColumnName("ID");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.AdminLogin)
					.HasMaxLength(255)
					.IsUnicode(false);
			});

			modelBuilder.Entity<SqlResourceStage>(entity =>
			{
				entity.ToTable("SqlResourceStage", "app");

				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).HasColumnName("ID");

				entity.Property(e => e.ResourceId)
					.ValueGeneratedNever()
					.HasColumnName("ResourceId");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.AdminLogin)
					.HasMaxLength(255)
					.IsUnicode(false);
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
