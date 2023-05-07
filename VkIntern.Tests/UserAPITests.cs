using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using VkIntern.API;
using VkIntern.API.Controllers;
using VkIntern.API.DbContexts;
using VkIntern.API.Models;
using VkIntern.API.Models.Dtos;
using VkIntern.API.Repository;

namespace VkIntern.Xunit
{
	public class UserAPITests
	{
		private static List<UserGroup> GetUserGroups()
			=> new List<UserGroup>
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

		private static List<UserState> GetUserStates()
			=> new List<UserState>
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

		private static List<User> GetUsers()
			=> new List<User>
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

		private static UserAPIController GetControllerMock()
		{
			var userGroupsList = GetUserGroups().AsQueryable();
			var userGroupsMock = new Mock<DbSet<UserGroup>>();
			userGroupsMock.As<IDbAsyncEnumerable<UserGroup>>()
				.Setup(m => m.GetAsyncEnumerator())
				.Returns(new TestDbAsyncEnumerator<UserGroup>(userGroupsList.GetEnumerator()));
			userGroupsMock.As<IQueryable<UserGroup>>()
				.Setup(m => m.Provider)
				.Returns(new TestDbAsyncQueryProvider<UserGroup>(userGroupsList.Provider));
			userGroupsMock.As<IQueryable<UserGroup>>()
				.Setup(m => m.Expression)
				.Returns(userGroupsList.Expression);
			userGroupsMock.As<IQueryable<UserGroup>>()
				.Setup(m => m.ElementType)
				.Returns(userGroupsList.ElementType);
			userGroupsMock.As<IQueryable<UserGroup>>()
				.Setup(m => m.GetEnumerator())
				.Returns(() => userGroupsList.GetEnumerator());

			var userStatesList = GetUserStates().AsQueryable();
			var userStatesMock = new Mock<DbSet<UserState>>();
			userStatesMock.As<IDbAsyncEnumerable<UserState>>()
				.Setup(m => m.GetAsyncEnumerator())
				.Returns(new TestDbAsyncEnumerator<UserState>(userStatesList.GetEnumerator()));
			userStatesMock.As<IQueryable<UserState>>()
				.Setup(m => m.Provider)
				.Returns(new TestDbAsyncQueryProvider<UserState>(userStatesList.Provider));
			userStatesMock.As<IQueryable<UserState>>()
				.Setup(m => m.Expression)
				.Returns(userStatesList.Expression);
			userStatesMock.As<IQueryable<UserState>>()
				.Setup(m => m.ElementType)
				.Returns(userStatesList.ElementType);
			userStatesMock.As<IQueryable<UserState>>()
				.Setup(m => m.GetEnumerator())
				.Returns(() => userStatesList.GetEnumerator());

			var usersList = GetUsers().AsQueryable();
			var usersMock = new Mock<DbSet<User>>();
			usersMock.As<IDbAsyncEnumerable<User>>()
				.Setup(m => m.GetAsyncEnumerator())
				.Returns(new TestDbAsyncEnumerator<User>(usersList.GetEnumerator()));
			usersMock.As<IQueryable<User>>()
				.Setup(m => m.Provider)
				.Returns(new TestDbAsyncQueryProvider<User>(usersList.Provider));
			usersMock.As<IQueryable<User>>()
				.Setup(m => m.Expression)
				.Returns(usersList.Expression);
			usersMock.As<IQueryable<User>>()
				.Setup(m => m.ElementType)
				.Returns(usersList.ElementType);
			usersMock.As<IQueryable<User>>()
				.Setup(m => m.GetEnumerator())
				.Returns(() => usersList.GetEnumerator());

			var dbContextMock = new Mock<ApplicationDbContext>();
			dbContextMock.Setup(c => c.UserGroups).Returns(userGroupsMock.Object);
			dbContextMock.Setup(c => c.UserStates).Returns(userStatesMock.Object);
			dbContextMock.Setup(c => c.Users).Returns(usersMock.Object);

			IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
			var userRepository = new UserRepository(mapper, dbContextMock.Object);
			var userApiController = new UserAPIController(userRepository);

			return userApiController;
		}

		private static Tuple<string, string, string> ParseSummary(
			List<UserSummaryDto> usersSummary)
		{
			var actualUserGroups = JsonConvert.SerializeObject(usersSummary
				.Select(x => new UserGroup
				{
					Id = x.UserGroupId,
					Code = x.UserGroupCode,
					Description = x.UserGroupDescription
				}).DistinctBy(x => new { x.Id, x.Code, x.Description }).ToList());

			var actualUserStates = JsonConvert.SerializeObject(usersSummary
				.Select(x => new UserState
				{
					Id = x.UserStateId,
					Code = x.UserStateCode,
					Description = x.UserStateDescription
				}).DistinctBy(x => new { x.Id, x.Code, x.Description }).ToList());

			var actualUsers = JsonConvert.SerializeObject(usersSummary
				.Select(x => new User
				{
					Id = x.Id,
					Login = x.Login,
					Password = x.Password,
					CreatedDate = x.CreatedDate,
					UserGroupId = x.UserGroupId,
					UserStateId = x.UserStateId
				}).ToList());

			return Tuple.Create(actualUserGroups, actualUserStates, actualUsers);
		}

		[Fact]
		public void AllApiMethodsAreAsync()
		{
			Type controllerType = typeof(UserAPIController);
			var apiMethods = controllerType.GetMethods()
				.Where(x =>
					x.GetCustomAttributes(typeof(HttpGetAttribute)).Any() ||
					x.GetCustomAttributes(typeof(HttpPostAttribute)).Any() ||
					x.GetCustomAttributes(typeof(HttpDeleteAttribute)).Any())
				.ToList();

			Type asyncType = typeof(AsyncStateMachineAttribute);

			Assert.All(apiMethods, method => Assert.NotNull(method.CustomAttributes
					.FirstOrDefault(attribute =>
						attribute.AttributeType.Name == asyncType.Name)));
		}

		[Fact]
		public async void GetAllUsersInfoWorks()
		{
			var controller = GetControllerMock();
			var response = await controller.Get();

			var usersSummaryList = (List<UserSummaryDto>)response.Result;

			var jsonTuple = ParseSummary(usersSummaryList);

			var actualUserGroups = jsonTuple.Item1;
			var actualUserStates = jsonTuple.Item2;
			var actualUsers = jsonTuple.Item3;

			var expectedUsers = JsonConvert.SerializeObject(GetUsers());
			var expectedUserStates = JsonConvert.SerializeObject(GetUserStates());
			var expectedUserGroups = JsonConvert.SerializeObject(GetUserGroups());

			Assert.Equal(expectedUsers, actualUsers);
			Assert.Equal(expectedUserStates, actualUserStates);
			Assert.Equal(expectedUserGroups, actualUserGroups);
		}

		[Theory]
		[InlineData(3, 0)]
		[InlineData(3, 2)]
		[InlineData(1, 3)]
		[InlineData(1, 1)]
		[InlineData(5, 0)]
		[InlineData(3444, 21221)]
		public async void GetAllUsersInfoWithLimitAndOffsetWorksWithCorrectInput(int limit, int offset)
		{
			var controller = GetControllerMock();
			var response = await controller.Get(limit, offset);

			var usersSummaryList = (List<UserSummaryDto>)response.Result;

			var jsonTuple = ParseSummary(usersSummaryList);

			var actualUserGroups = jsonTuple.Item1;
			var actualUserStates = jsonTuple.Item2;
			var actualUsers = jsonTuple.Item3;

			if (offset > 0)
				offset++;
			var idsRange = Enumerable.Range(offset, limit).ToHashSet();

			var expectedUsersList = GetUsers()
				.Where(x => idsRange.Contains(x.Id))
				.OrderBy(x => x.Id)
				.ToList();

			var expectedUserStateIds = expectedUsersList.Select(x => x.UserStateId)
				.ToHashSet();

			var expectedUserGroupIds = expectedUsersList.Select(x => x.UserGroupId)
				.ToHashSet();

			var expectedUsers = JsonConvert.SerializeObject(expectedUsersList);
			var expectedUserStates = JsonConvert.SerializeObject(
				GetUserStates()
				.Where(x => expectedUserStateIds.Contains(x.Id))
				.OrderBy(x => x.Id)
				.ToList());
			var expectedUserGroups = JsonConvert.SerializeObject(
				GetUserGroups()
				.Where(x => expectedUserGroupIds.Contains(x.Id))
				.OrderBy(x => x.Id)
				.ToList());

			Assert.Equal(expectedUsers, actualUsers);
			Assert.Equal(expectedUserStates, actualUserStates);
			Assert.Equal(expectedUserGroups, actualUserGroups);
		}

		[Theory]
		[InlineData(0, 1)]
		[InlineData(-2, 4)]
		[InlineData(-100, -8)]
		[InlineData(2, -3)]
		public async void GetAllUsersInfoWithLimitAndOffsetFailsWithIncorrectInput(int limit, int offset)
		{
			var controller = GetControllerMock();
			var response = await controller.Get(limit, offset);

			Assert.True(response.ErrorMessages.Count > 0 && response.Result == null);
		}

		[Fact]
		public async void GetUsersInfoByIdWorksWithIncorrectInput()
		{
			var controller = GetControllerMock();

			for (int id = 1; id <= GetUsers().Count; id++)
			{
				var response = await controller.Get(id);
				var userSummary = (List<UserSummaryDto>)response.Result;
				var jsonTuple = ParseSummary(userSummary);

				var actualUserGroup = jsonTuple.Item1;
				var actualUserState = jsonTuple.Item2;
				var actualUser = jsonTuple.Item3;

				var expectedUser = GetUsers().FirstOrDefault(x => x.Id == id);
				var expectedUserState = GetUserStates().FirstOrDefault(x => x.Id == expectedUser.UserStateId);
				var expectedUserGroup = GetUserGroups().FirstOrDefault(x => x.Id == expectedUser.UserGroupId);

				Assert.Equal(JsonConvert.SerializeObject(expectedUser), actualUser);
				Assert.Equal(JsonConvert.SerializeObject(expectedUserState), actualUserState);
				Assert.Equal(JsonConvert.SerializeObject(expectedUserGroup), actualUserGroup);
			}
		}
	}
}