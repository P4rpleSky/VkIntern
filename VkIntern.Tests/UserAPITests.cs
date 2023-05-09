using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Runtime.CompilerServices;
using VkIntern.API.Controllers;
using VkIntern.API.Models;
using VkIntern.API.Models.Dtos;

namespace VkIntern.Tests
{
	public class UserAPITests
	{
		private static (string, string, string) ParseUserSummaryList(
			List<UserSummaryDto> usersSummary)
		{
			var actualUsers = usersSummary.Select(x => new User
			{
				Id = x.Id,
				Login = x.Login,
				Password = x.Password,
				CreatedDate = x.CreatedDate,
				UserGroupId = x.UserGroupId,
				UserGroup = new UserGroup
				{
					Id = x.UserGroupId,
					Code = x.UserGroupCode,
					Description = x.UserGroupDescription
				},
				UserStateId = x.UserStateId,
				UserState = new UserState
				{
					Id = x.UserStateId,
					Code = x.UserStateCode,
					Description = x.UserStateDescription
				}
			}).ToList();

			var actualUserGroups = usersSummary.Select(x => new UserGroup
			{
				Id = x.UserGroupId,
				Code = x.UserGroupCode,
				Description = x.UserGroupDescription,
			}).DistinctBy(x => new { x.Id, x.Code, x.Description }).ToList();

			var actualUserStates = usersSummary.Select(x => new UserState
			{
				Id = x.UserStateId,
				Code = x.UserStateCode,
				Description = x.UserStateDescription,
			}).DistinctBy(x => new { x.Id, x.Code, x.Description }).ToList();

			return (
				actualUserGroups.SerializeToJSON(), 
				actualUserStates.SerializeToJSON(), 
				actualUsers.SerializeToJSON());
		}

		private static (string, string, string) ParseDbContext(
			IEnumerable<UserGroup> userGroupsSet,
			IEnumerable<UserState> userStatesSet,
			IEnumerable<User> usersSet)
		{

			var userGroupsList = new List<UserGroup>(userGroupsSet);
			var userStatesList = new List<UserState>(userStatesSet);
			var usersList = new List<User>(usersSet);


			foreach (var userGroup in userGroupsList)
			{
				userGroup.Users.Clear();
			}

			foreach (var userState in userStatesList)
			{
				userState.Users.Clear();
			}

			foreach (var user in usersList)
			{
				user.UserGroup.Users.Clear();
				user.UserState.Users.Clear();
			}

			return (
				userGroupsList.SerializeToJSON(),
				userStatesList.SerializeToJSON(),
				usersList.SerializeToJSON());
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
		public async Task GetAllUsersSummaryWorks()
		{
			var emulator = new ControllerEmulator();
			
			var response = await emulator.Controller.Get();

			var usersSummaryList = response.Result as List<UserSummaryDto>;
			Assert.NotNull(usersSummaryList);
			var (actualUserGroups, actualUserStates, actualUsers) = ParseUserSummaryList(usersSummaryList);

			var expectedUsersRaw = emulator.DbContext.Users;
			var expectedUserStatesRaw = emulator.DbContext.UserStates;
			var expectedUserGroupsRaw = emulator.DbContext.UserGroups;
			var (expectedUserGroups, expectedUserStates, expectedUsers) = ParseDbContext(
				expectedUserGroupsRaw,
				expectedUserStatesRaw,
				expectedUsersRaw);


			Assert.Equal(expectedUsers, actualUsers);
			Assert.Equal(expectedUserStates, actualUserStates);
			Assert.Equal(expectedUserGroups, actualUserGroups);
			Assert.True(response.ErrorMessages.Count == 0);
		}

		[Theory]
		[InlineData(3, 0)]
		[InlineData(3, 2)]
		[InlineData(1, 3)]
		[InlineData(1, 1)]
		[InlineData(5, 0)]
		[InlineData(3444, 21221)]
		public async Task PaginationWorksWithCorrectInput(int limit, int offset)
		{
			var emulator = new ControllerEmulator();

			var response = await emulator.Controller.Get(limit, offset);

			var usersSummaryList = response.Result as List<UserSummaryDto>;
			Assert.NotNull(usersSummaryList);

			var (actualUserGroups, actualUserStates, actualUsers) = ParseUserSummaryList(usersSummaryList);

			if (offset > 0)
				offset++;
			var idsRange = Enumerable.Range(offset, limit).ToHashSet();

			var expectedUsersList = emulator.DbContext.Users
				.Where(x => idsRange.Contains(x.Id))
				.OrderBy(x => x.Id)
				.ToList();

			var expectedUserStateIds = expectedUsersList.Select(x => x.UserStateId)
				.ToHashSet();
			var expectedUserGroupIds = expectedUsersList.Select(x => x.UserGroupId)
				.ToHashSet();

			var expectedUsersRaw = expectedUsersList;
			var expectedUserStatesRaw = emulator.DbContext.UserStates
				.Where(x => expectedUserStateIds.Contains(x.Id))
				.OrderBy(x => x.Id);
			var expectedUserGroupsRaw = emulator.DbContext.UserGroups
				.Where(x => expectedUserGroupIds.Contains(x.Id))
				.OrderBy(x => x.Id);

			var (expectedUserGroups, expectedUserStates, expectedUsers) = ParseDbContext(
				expectedUserGroupsRaw,
				expectedUserStatesRaw,
				expectedUsersRaw);

			Assert.Equal(expectedUsers, actualUsers);
			Assert.Equal(expectedUserStates, actualUserStates);
			Assert.Equal(expectedUserGroups, actualUserGroups);
			Assert.True(response.ErrorMessages.Count == 0);
		}

		[Theory]
		[InlineData(0, 1)]
		[InlineData(-2, 4)]
		[InlineData(-100, -8)]
		[InlineData(2, -3)]
		public async Task PaginationFailsWithIncorrectInput(int limit, int offset)
		{
			var emulator = new ControllerEmulator();

			var response = await emulator.Controller.Get(limit, offset);
			Assert.True(response.ErrorMessages.Any());
			Assert.Null(response.Result);
		}

		[Fact]
		public async Task GetUserInfoByIdWorksWithCorrectId()
		{
			var emulator = new ControllerEmulator();

			for (int id = 1; id <= emulator.DbContext.Users.Count(); id++)
			{
				var response = await emulator.Controller.Get(id);
				var userSummaryDto = response.Result as UserSummaryDto;
				Assert.NotNull(userSummaryDto);

				var userSummary = new List<UserSummaryDto> { userSummaryDto };
				var (actualUserGroup, actualUserState, actualUser) = ParseUserSummaryList(userSummary);

				var expectedUsersRaw = emulator.DbContext.Users
					.Where(x => x.Id == id);
				var expectedUserStatesRaw = emulator.DbContext.UserStates
					.Where(x => x.Id == userSummaryDto.UserStateId);
				var expectedUserGroupsRaw = emulator.DbContext.UserGroups
					.Where(x => x.Id == userSummaryDto.UserGroupId);

				var (expectedUserGroups, expectedUserStates, expectedUsers) = ParseDbContext(
					expectedUserGroupsRaw,
					expectedUserStatesRaw,
					expectedUsersRaw);

				Assert.Equal(expectedUsers, actualUser);
				Assert.Equal(expectedUserStates, actualUserState);
				Assert.Equal(expectedUserGroups, actualUserGroup);
				Assert.True(response.ErrorMessages.Count == 0);
			}
		}

		[Fact]
		public async Task GetUserInfoByIdFailsWithIncorrectId()
		{
			var emulator = new ControllerEmulator();

			var trueRange = Enumerable.Range(1, emulator.DbContext.Users.Count()).ToHashSet();
			var falseRange = new List<int>();
			Random rnd = new Random();
			for (int i = 0; i < 1000; i++)
				falseRange.Add(rnd.Next(-100000, 200000));

			falseRange = falseRange.Where(x => !trueRange.Contains(x)).ToList();
			for (int i = 0; i < falseRange.Count; i++)
			{
				int id = falseRange[i];
				var response = await emulator.Controller.Get(id);
				Assert.True(response.ErrorMessages.Any());
				Assert.Null(response.Result);
			}
		}

		[Fact]
		public async Task DeleteUserByIdWorksWithCorrectId()
		{
			var emulator = new ControllerEmulator();

			var idsToDelete = emulator.DbContext.Users.Where(x =>
				x.UserStateId == emulator.DbContext.UserStates.Where(x => x.Code == "Active").First().Id)
				.Select(x => x.Id)
				.ToList();
			var blockedStateId = emulator.DbContext.UserStates.Where(x => x.Code == "Blocked").First().Id;
			foreach (var id in idsToDelete)
			{
				var responseOnDelete = await emulator.Controller.Delete(id);
				var responseOnGet = await emulator.Controller.Get(id);

				var userSummaryDto = responseOnGet.Result as UserSummaryDto;
				
				Assert.NotNull(userSummaryDto);
				Assert.Equal(blockedStateId, userSummaryDto.UserStateId);
				Assert.True(responseOnDelete.ErrorMessages.Count == 0);
			}
		}

		[Fact]
		public async void DeleteUserByIdFailsWithIncorrectId()
		{
			var emulator = new ControllerEmulator();

			var trueRange = Enumerable.Range(1, emulator.DbContext.Users.Count()).ToHashSet();
			var falseRange = new List<int>();
			Random rnd = new Random();
			for (int i = 0; i < 1000; i++)
				falseRange.Add(rnd.Next(-100000, 200000));

			falseRange = falseRange.Where(x => !trueRange.Contains(x)).ToList();
			for (int i = 0; i < falseRange.Count; i++)
			{
				int id = falseRange[i];
				var response = await emulator.Controller.Get(id);
				Assert.True(response.ErrorMessages.Any());
				Assert.Null(response.Result);
			}
		}

		[Fact]
		public async void DeleteUserByIdFailsOnAlreadyDeletedUser()
		{
			var emulator = new ControllerEmulator();
			var incorrectUserIdsList = new List<int>();
			foreach (var user in emulator.DbContext.Users.Where(x => x.UserState.Code == "Blocked"))
			{
				incorrectUserIdsList.Add(user.Id);
			}

			foreach (var id in incorrectUserIdsList)
			{
				var response = await emulator.Controller.Delete(id);
				Assert.True(response.ErrorMessages.Any());
				Assert.Null(response.Result);
			}
		}

		[Theory]
		[InlineData("Sanya", "221aslla", "User")]
		[InlineData("Kate", "836", "User")]
		[InlineData("YaKtoPomogite", "aasasas", "User")]
		[InlineData("228proger1488", "656asa", "User")]
		public async Task CreateUserWorksWithCorrectInput(
			string login,
			string password,
			string userGroupCode)
		{
			var emulator = new ControllerEmulator();
			var expectedUserDto = new UserDto
			{
				Login = login,
				Password = password,
				UserGroupCode = userGroupCode
			};

			var responseOnPost = await emulator.Controller.Post(expectedUserDto);
			var actualUserDto = responseOnPost.Result as UserDto;
			Assert.NotNull(actualUserDto);

			Assert.Equal(
				expectedUserDto.SerializeToJSON(), 
				actualUserDto.SerializeToJSON());

			Assert.True(responseOnPost.ErrorMessages.Count == 0);

			var newUserId = emulator.DbContext.Users.Count();
			var responseOnGet = await emulator.Controller.Get(newUserId);
			var userSummary = responseOnGet.Result as UserSummaryDto;

			var usersSummary = new List<UserSummaryDto> { userSummary };
			var (actualUserGroup, actualUserState, actualUser) = ParseUserSummaryList(usersSummary);

			var expectedUsersRaw = emulator.DbContext.Users
				.Where(x => x.Id == newUserId);
			var expectedUserStatesRaw = emulator.DbContext.UserStates
				.Where(x => x.Id == userSummary.UserStateId);
			var expectedUserGroupsRaw = emulator.DbContext.UserGroups
				.Where(x => x.Id == userSummary.UserGroupId);

			var (expectedUserGroups, expectedUserStates, expectedUsers) = ParseDbContext(
				expectedUserGroupsRaw,
				expectedUserStatesRaw,
				expectedUsersRaw);

			Assert.Equal(expectedUsers, actualUser);
			Assert.Equal(expectedUserStates, actualUserState);
			Assert.Equal(expectedUserGroups, actualUserGroup);
		}

		[Fact]
		public async void CreateAdminFailsWithAdminAlreadyInDb()
		{
			var emulator = new ControllerEmulator();

			var login = "new_first_admin";
			var password = "password";
			var userGroupCode = "Admin";

			var userDto = new UserDto
			{
				Login = login,
				Password = password,
				UserGroupCode = userGroupCode
			};

			var response = await emulator.Controller.Post(userDto);

			Assert.True(response.ErrorMessages.Any());
			Assert.Null(response.Result);
		}

		[Fact]
		public async void CreateAdminWorksWithDeletedAdminInDb()
		{
			var emulator = new ControllerEmulator();

			var login = "new_first_admin";
			var password = "password";
			var userGroupCode = "Admin";

			var expectedUserDto = new UserDto
			{
				Login = login,
				Password = password,
				UserGroupCode = userGroupCode
			};
			var activeAdminId = emulator.DbContext.Users.First(x => x.UserGroup.Code == "Admin").Id;
			_ = await emulator.Controller.Delete(activeAdminId);

			var response = await emulator.Controller.Post(expectedUserDto);

			var actualUserDto = response.Result as UserDto;

			Assert.Equal(
				expectedUserDto.SerializeToJSON(),
				actualUserDto.SerializeToJSON());
			Assert.True(response.ErrorMessages.Count == 0);

			var newUserId = emulator.DbContext.Users.Count();
			var responseOnGet = await emulator.Controller.Get(newUserId);
			var userSummary = responseOnGet.Result as UserSummaryDto;

			var usersSummary = new List<UserSummaryDto> { userSummary };
			var (actualUserGroup, actualUserState, actualUser) = ParseUserSummaryList(usersSummary);

			var expectedUsersRaw = emulator.DbContext.Users
				.Where(x => x.Id == newUserId);
			var expectedUserStatesRaw = emulator.DbContext.UserStates
				.Where(x => x.Id == userSummary.UserStateId);
			var expectedUserGroupsRaw = emulator.DbContext.UserGroups
				.Where(x => x.Id == userSummary.UserGroupId);


			var (expectedUserGroups, expectedUserStates, expectedUsers) = ParseDbContext(
				expectedUserGroupsRaw,
				expectedUserStatesRaw,
				expectedUsersRaw);

			Assert.Equal(expectedUsers, actualUser);
			Assert.Equal(expectedUserStates, actualUserState);
			Assert.Equal(expectedUserGroups, actualUserGroup);
		}

		[Fact]
		public async void CreateAdminWorksForDbWithoutAdmin()
		{
			var emulator = new ControllerEmulator(isFilledWithUsers: false);

			var login = "new_first_admin";
			var password = "password";
			var userGroupCode = "Admin";

			var expectedUserDto = new UserDto
			{
				Login = login,
				Password = password,
				UserGroupCode = userGroupCode
			};

			var responseOnPost = await emulator.Controller.Post(expectedUserDto);
			var actualUserDto = responseOnPost.Result as UserDto;

			Assert.Equal(
				expectedUserDto.SerializeToJSON(),
				actualUserDto.SerializeToJSON());
			Assert.True(responseOnPost.ErrorMessages.Count == 0);

			var newUserId = emulator.DbContext.Users.Count();
			var responseOnGet = await emulator.Controller.Get(newUserId);
			var userSummary = responseOnGet.Result as UserSummaryDto;

			var usersSummary = new List<UserSummaryDto> { userSummary };
			var (actualUserGroup, actualUserState, actualUser) = ParseUserSummaryList(usersSummary);

			var expectedUsersRaw = emulator.DbContext.Users
				.Where(x => x.Id == newUserId);
			var expectedUserStatesRaw = emulator.DbContext.UserStates
				.Where(x => x.Id == userSummary.UserStateId);
			var expectedUserGroupsRaw = emulator.DbContext.UserGroups
				.Where(x => x.Id == userSummary.UserGroupId);

			var (expectedUserGroups, expectedUserStates, expectedUsers) = ParseDbContext(
				expectedUserGroupsRaw,
				expectedUserStatesRaw,
				expectedUsersRaw);

			Assert.Equal(expectedUsers, actualUser);
			Assert.Equal(expectedUserStates, actualUserState);
			Assert.Equal(expectedUserGroups, actualUserGroup);
		}

		[Fact]
		public async Task CreateUserFailsWhenAddingWithExistingLogin()
		{
			var emulator = new ControllerEmulator();
			var incorrectUserDtoList = new List<UserDto>();
			foreach (var user in emulator.DbContext.Users)
			{
				incorrectUserDtoList.Add(new UserDto
				{
					Login = user.Login,
					Password = Guid.NewGuid().ToString(),
					UserGroupCode = "User"
				});
			}

			foreach (var userDto in incorrectUserDtoList)
			{
				var response = await emulator.Controller.Post(userDto);
				Assert.True(response.ErrorMessages.Any());
				Assert.Null(response.Result);
			}
		}

		[Fact]
		public async Task CreateUserFailsWhenAddingWithInvalidUserGroupCode()
		{
			var emulator = new ControllerEmulator();
			var incorrectUserDtoList = new List<UserDto>();
			for (int i = 0; i < 100; i++)
			{
				incorrectUserDtoList.Add(new UserDto
				{
					Login = Guid.NewGuid().ToString(),
					Password = Guid.NewGuid().ToString(),
					UserGroupCode = Guid.NewGuid().ToString()
				});
			}

			foreach (var userDto in incorrectUserDtoList)
			{
				var response = await emulator.Controller.Post(userDto);
				Assert.True(response.ErrorMessages.Any());
				Assert.Null(response.Result);
			}
		}
	}
}