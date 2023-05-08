using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VkIntern.API.Models;

namespace VkIntern.API.DbContexts;

public partial class ApplicationDbContext : DbContext
{
	public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<UserState> UserStates { get; set; }

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
	{

	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<UserGroup>().HasData(new UserGroup
		{
			Id = 1,
			Code = "Admin",
			Description = "I am admin!"
		});
		modelBuilder.Entity<UserGroup>().HasData(new UserGroup
		{
			Id = 2,
			Code = "User",
			Description = "I am default user!"
		});

		modelBuilder.Entity<UserState>().HasData(new UserState
		{
			Id = 1,
			Code = "Active",
			Description = "This user is active!"
		});
		modelBuilder.Entity<UserState>().HasData(new UserState
		{
			Id = 2,
			Code = "Blocked",
			Description = "This user is blocked!"
		});

		modelBuilder.Entity<User>().HasData(new User
		{
			Id = 1,
			Login = "admin",
			Password = "admin123",
			CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
			UserGroupId = 1,
			UserStateId = 1
		});
		modelBuilder.Entity<User>().HasData(new User
		{
			Id = 2,
			Login = "Alex",
			Password = "778877",
			CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
			UserGroupId = 2,
			UserStateId = 1
		});
		modelBuilder.Entity<User>().HasData(new User
		{
			Id = 3,
			Login = "Jon",
			Password = "11231",
			CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
			UserGroupId = 2,
			UserStateId = 2
		});
	}
}