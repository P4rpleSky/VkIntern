using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VkIntern.API.Models;

namespace VkIntern.API.DbContexts;

public partial class ApplicationDbContext : DbContext
{
	public ApplicationDbContext()
	{
	}

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public virtual DbSet<User> Users { get; set; }

	public virtual DbSet<UserGroup> UserGroups { get; set; }

	public virtual DbSet<UserState> UserStates { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("user_pkey");

			entity.ToTable("user");

			entity.HasIndex(e => e.Login, "user_login_key").IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.CreatedDate).HasColumnName("created_date");
			entity.Property(e => e.Login)
				.HasMaxLength(16)
				.HasColumnName("login");
			entity.Property(e => e.Password)
				.HasMaxLength(16)
				.HasColumnName("password");
			entity.Property(e => e.UserGroupId).HasColumnName("user_group_id");
			entity.Property(e => e.UserStateId).HasColumnName("user_state_id");

			entity.HasOne(d => d.UserGroup).WithMany(p => p.Users)
				.HasForeignKey(d => d.UserGroupId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("fk_user_group_id");

			entity.HasOne(d => d.UserState).WithMany(p => p.Users)
				.HasForeignKey(d => d.UserStateId)
				.OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("fk_user_state_id");
		});

		modelBuilder.Entity<UserGroup>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("user_group_pkey");

			entity.ToTable("user_group");

			entity.HasIndex(e => e.Code, "user_group_code_key").IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.Code)
				.HasMaxLength(16)
				.HasColumnName("code");
			entity.Property(e => e.Description)
				.HasMaxLength(128)
				.HasColumnName("description");
		});

		modelBuilder.Entity<UserState>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("user_state_pkey");

			entity.ToTable("user_state");

			entity.HasIndex(e => e.Code, "user_state_code_key").IsUnique();

			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.Code)
				.HasMaxLength(16)
				.HasColumnName("code");
			entity.Property(e => e.Description)
				.HasMaxLength(128)
				.HasColumnName("description");
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}