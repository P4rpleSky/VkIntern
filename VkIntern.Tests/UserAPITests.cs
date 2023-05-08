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
using VkIntern.Tests;

namespace VkIntern.Tests
{
	public class UserAPITests
	{
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
		public async Task GetAllUsersInfoWorks()
		{
			var emulator = new ControllerEmulator();
			
			var response = await emulator.Controller.Get();

			var usersSummaryList = (List<UserSummaryDto>)response.Result;

			var jsonTuple = ParseSummary(usersSummaryList);

			var actualUserGroups = jsonTuple.Item1;
			var actualUserStates = jsonTuple.Item2;
			var actualUsers = jsonTuple.Item3;

			var expectedUsers = JsonConvert.SerializeObject(emulator.DbContext.Users);
			var expectedUserStates = JsonConvert.SerializeObject(emulator.DbContext.UserStates);
			var expectedUserGroups = JsonConvert.SerializeObject(emulator.DbContext.UserGroups);

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
		public async Task PaginationWorksWithCorrectInput(int limit, int offset)
		{
			var emulator = new ControllerEmulator();

			var response = await emulator.Controller.Get(limit, offset);

			var usersSummaryList = (List<UserSummaryDto>)response.Result;

			var jsonTuple = ParseSummary(usersSummaryList);

			var actualUserGroups = jsonTuple.Item1;
			var actualUserStates = jsonTuple.Item2;
			var actualUsers = jsonTuple.Item3;

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

			var expectedUsers = JsonConvert.SerializeObject(expectedUsersList);
			var expectedUserStates = JsonConvert.SerializeObject(
				emulator.DbContext.UserStates
				.Where(x => expectedUserStateIds.Contains(x.Id))
				.OrderBy(x => x.Id)
				.ToList());
			var expectedUserGroups = JsonConvert.SerializeObject(
				emulator.DbContext.UserGroups
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
		public async Task PaginationFailsWithIncorrectInput(int limit, int offset)
		{
			var emulator = new ControllerEmulator();

			var response = await emulator.Controller.Get(limit, offset);
			Assert.True(response.ErrorMessages.Count > 0 && response.Result == null);
		}

		[Fact]
		public async Task GetUsersInfoByIdWorksWithCorrectInput()
		{
			var emulator = new ControllerEmulator();

			for (int id = 1; id <= emulator.DbContext.Users.Count(); id++)
			{
				var response = await emulator.Controller.Get(id);
				var userSummary = new List<UserSummaryDto> { (UserSummaryDto)response.Result };
				var jsonTuple = ParseSummary(userSummary);

				var actualUserGroup = jsonTuple.Item1;
				var actualUserState = jsonTuple.Item2;
				var actualUser = jsonTuple.Item3;

				var expectedUser = emulator.DbContext.Users.Where(x => x.Id == id).ToList();
				var expectedUserState = emulator.DbContext.UserStates.Where(x => x.Id == expectedUser[0].UserStateId).ToList();
				var expectedUserGroup = emulator.DbContext.UserGroups.Where(x => x.Id == expectedUser[0].UserGroupId).ToList();

				Assert.Equal(JsonConvert.SerializeObject(expectedUser), actualUser);
				Assert.Equal(JsonConvert.SerializeObject(expectedUserState), actualUserState);
				Assert.Equal(JsonConvert.SerializeObject(expectedUserGroup), actualUserGroup);
			}
		}

		[Fact]
		public async Task GetUsersInfoByIdFailsWithIncorrectInput()
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
				Assert.True(response.ErrorMessages.Count > 0 && response.Result == null);
			}
		}

		[Fact]
		public async Task DeleteUserByIdWorksWithCorrectInput()
		{
			var emulator = new ControllerEmulator();

			var idsToDelete = emulator.DbContext.Users.Where(x =>
				x.UserStateId == emulator.DbContext.UserStates.Where(x => x.Code == "Active").First().Id)
				.Select(x => x.Id)
				.ToList();
			var blockedStateId = emulator.DbContext.UserStates.Where(x => x.Code == "Blocked").First().Id;
			foreach (var id in idsToDelete)
			{
				var response1 = await emulator.Controller.Delete(id);
				var response2 = (UserSummaryDto)(await emulator.Controller.Get(id)).Result;
				Assert.Equal(blockedStateId, response2.UserStateId);
			}
		}

		[Fact]
		public async void DeleteUserByIdFailsWithIncorrectInput()
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
				Assert.True(response.ErrorMessages.Count > 0 && response.Result == null);
			}
		}

		
	}
}