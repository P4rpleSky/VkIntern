using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VkIntern.API.DbContexts;
using VkIntern.API.Models;
using VkIntern.API.Models.Dtos;

namespace VkIntern.API.Repository
{
    public class UserRepository : IUserRepository
	{
		private readonly IMapper _mapper;
		private readonly ApplicationDbContext _db;

		public UserRepository(IMapper mapper, ApplicationDbContext db)
		{
			_db = db;
			_mapper = mapper;
		}

		public async Task<UserDto> CreateUserAsync(UserDto userDto)
		{
			if (_db.GetUsersSummary().Any(x => x.Login == userDto.Login))
			{
				throw new Exception($"User with login {userDto.Login} already exist!");
			}

			var adminGroupCode = "Admin";
			if (userDto.UserGroupCode == adminGroupCode && 
				_db.GetUsersSummary().Where(x => x.UserGroup.Code == adminGroupCode).Any())
			{
				throw new Exception("Cannot add another admin!");
			}

			var user = _mapper.Map<UserDto, User>(userDto);

			var activeStateCode = "Active";
			var activeUserState = await _db.UserStates.FirstOrDefaultAsync(x => x.Code == activeStateCode);
			if (activeUserState == null)
				throw new Exception($"User state with code \"{activeStateCode}\" is not defined!");
			user.UserStateId = activeUserState.Id;

			var userGroup = await _db.UserGroups.FirstOrDefaultAsync(x => x.Code == userDto.UserGroupCode);
			if (userGroup == null)
				throw new Exception($"User group with code \"{userDto.UserGroupCode}\" is not defined!");
			user.UserGroupId = userGroup.Id;

			user.CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow);

			_db.Users.Add(user);
			await _db.SaveChangesAsync();
			return _mapper.Map<User, UserDto>(user);
		}

		public async Task<bool> DeleteUserAsync(int userId)
		{
			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
			if (user == null)
				throw new IndexOutOfRangeException($"User with id {userId} is not exists!");

			var blockedStateCode = "Blocked";
			var blockedUserState = await _db.UserStates.FirstOrDefaultAsync(x => x.Code == blockedStateCode);
			if (blockedUserState == null)
				throw new Exception($"User state with code \"{blockedStateCode}\" is not defined!");

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
			var usersInfoList = await _db.GetUsersSummary().ToListAsync();
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
