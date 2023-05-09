using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq;
using VkIntern.API.DbContexts;
using VkIntern.API.Login;
using VkIntern.API.Models;
using VkIntern.API.Models.Dtos;

namespace VkIntern.API.Repository
{
    public class UserRepository : IUserRepository
	{
		private readonly IMapper _mapper;
		private readonly ILoginHandler _loginHandler;
		private readonly ApplicationDbContext _db;
		

		public UserRepository(IMapper mapper, ILoginHandler loginHandler, ApplicationDbContext db)
		{
			_db = db;
			_loginHandler = loginHandler;
			_mapper = mapper;
		}

		public async Task<UserDto> CreateUserAsync(UserDto userDto)
		{
			CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

			var creationDelayTask = Task.Delay(5000, cancelTokenSource.Token);
			if (_loginHandler.IsInRecent(userDto.Login))
			{
				_loginHandler.GetToken(userDto.Login).Cancel();
				_loginHandler.RemoveLogin(userDto.Login);
				throw new ArgumentException(
					$"Failed to create user with login {userDto.Login} because someone tried to do it too in the last 5 seconds!");
			}
			_loginHandler.AddLoginWithToken(userDto.Login, cancelTokenSource);

			if (_db.GetUsersSummary().Any(x => x.Login == userDto.Login))
			{
				throw new Exception($"User with login {userDto.Login} already exists!");
			}

			var adminGroupCode = "Admin";
			if (userDto.UserGroupCode == adminGroupCode &&
				_db.GetUsersSummary()
				.Where(x => x.UserGroup.Code == adminGroupCode && x.UserState.Code == "Active")
				.Any())
			{
				throw new Exception("Cannot add another admin!");
			}

			var user = _mapper.Map<UserDto, User>(userDto);

			var activeStateCode = "Active";
			var activeUserState = await _db.UserStates.FirstAsync(x => x.Code == activeStateCode);
			user.UserStateId = activeUserState.Id;

			var userGroup = await _db.UserGroups.FirstAsync(x => x.Code == userDto.UserGroupCode);
			user.UserGroupId = userGroup.Id;
			user.CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow);

			try
			{
				await creationDelayTask;
			}
			catch 
			{
				throw new TaskCanceledException(
					$"Failed to create user with login {userDto.Login} because someone tried to do it too in the next 5 seconds!");
			}
			
			_loginHandler.RemoveLogin(userDto.Login);
			_db.Users.Add(user);
			await _db.SaveChangesAsync();
			return _mapper.Map<User, UserDto>(user);
		}

		public async Task<bool> DeleteUserAsync(int userId)
		{
			var blockedStateCode = "Blocked"; 
			var fullUserInfo = await _db.GetUsersSummary().FirstOrDefaultAsync(x => x.Id == userId);
			if (fullUserInfo == null)
				throw new IndexOutOfRangeException($"User with id {userId} is not exists!");
			else if (fullUserInfo.UserState.Code == blockedStateCode)
				throw new IndexOutOfRangeException($"User with id {userId} is already deleted!");

			var blockedUserState = await _db.UserStates.FirstAsync(x => x.Code == blockedStateCode);

			var user = await _db.Users.FirstAsync(x => x.Id == userId);
			user.UserStateId = blockedUserState.Id;
			await _db.SaveChangesAsync();

			return true;
		}

		public async Task<UserSummaryDto> GetUserInfoByIdAsync(int userId)
		{
			var userFullInfo = await _db.GetUsersSummary().FirstOrDefaultAsync(x => x.Id == userId);
			if (userFullInfo == null)
				throw new IndexOutOfRangeException($"User with id {userId} is not exists!");

			return _mapper.Map<UserSummaryDto>(userFullInfo);
		}

		public async Task<IEnumerable<UserSummaryDto>> GetUsersInfoAsync()
		{
			var usersInfoList = await _db.GetUsersSummary().OrderBy(x => x.Id).ToListAsync();
			return _mapper.Map<List<UserSummaryDto>>(usersInfoList);
		}

		public async Task<IEnumerable<UserSummaryDto>> GetUsersInfoAsync(int limit, int offset)
		{
			if (limit <= 0)
				throw new ArgumentException($"Limit cannot be less than or equal to zero!");

			if (offset < 0)
				throw new ArgumentException($"Offset cannot be less than zero!");

			if (offset > 0)
				offset++;

			var idsRange = Enumerable.Range(offset, limit).ToHashSet();
			var usersInfoList = await _db.GetUsersSummary()
				.Where(x => idsRange.Contains(x.Id))
				.OrderBy(x => x.Id)
				.ToListAsync();
			return _mapper.Map<List<UserSummaryDto>>(usersInfoList);
		}
	}
}
