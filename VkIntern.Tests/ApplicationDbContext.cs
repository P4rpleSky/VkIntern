using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkIntern.API.Controllers;
using VkIntern.API;
using VkIntern.API.DbContexts;
using VkIntern.API.Models;
using VkIntern.API.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using VkIntern.API.Login;

namespace VkIntern.Tests
{
	public class ControllerEmulator
	{
		public ApplicationDbContext DbContext { get; }

		public UserAPIController Controller { get; }

		public ControllerEmulator(bool isFilledWithUsers = true)
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "userdb_mock" + Guid.NewGuid())
				.Options;

			DbContext = new ApplicationDbContext(options);
			if (isFilledWithUsers)
			{
				var users = new List<User>
				{
					new User
					{
						Id = 1,
						Login = "admin",
						Password = "admin123",
						CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
						UserGroupId = 1,
						UserStateId = 1
					},
					new User
					{
						Id = 2,
						Login = "Alex",
						Password = "332323",
						CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
						UserGroupId = 2,
						UserStateId = 1
					},
					new User
					{
						Id = 3,
						Login = "Jon",
						Password = "11231",
						CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
						UserGroupId = 2,
						UserStateId = 2
					}
				};

				DbContext.Users.AddRange(users);
			}

			var userGroups = new List<UserGroup>
			{
				new UserGroup
				{
					Id = 1,
					Code = "Admin",
					Description = "Admin in fake db!"
				},
				new UserGroup
				{
					Id= 2,
					Code = "User",
					Description = "User in fake db!"
				}
			};

			var userStates = new List<UserState>
			{
				new UserState
				{
					Id = 1,
					Code = "Active",
					Description = "This user is active in fake db!"
				},
				new UserState
				{
					Id= 2,
					Code = "Blocked",
					Description = "This user is blocked in fake db!"
				}
			};

			DbContext.UserGroups.AddRange(userGroups);
			DbContext.UserStates.AddRange(userStates);
			DbContext.SaveChanges();

			IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
			ILoginHandler loginHandler = new LoginHandler();
			var userRepository = new UserRepository(mapper, loginHandler, DbContext);
			Controller = new UserAPIController(userRepository);
		}

		//public void Dispose()
		//{
		//	DbContext.Database.EnsureDeleted();
		//	DbContext.Dispose();
		//}
	}
}
