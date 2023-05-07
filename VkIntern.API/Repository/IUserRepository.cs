using VkIntern.API.Models.Dtos;

namespace VkIntern.API.Repository
{
	public interface IUserRepository
	{
		Task<IEnumerable<UserSummaryDto>> GetUsersInfoAsync();
		Task<UserSummaryDto> GetUserInfoByIdAsync(int userId);
		Task<bool> DeleteUserAsync(int userId);
		Task<UserDto> CreateUserAsync(UserDto userDto);
		Task<IEnumerable<UserSummaryDto>> GetUsersInfoAsync(int limit, int offset);
		//Task<UserDto> CreateUpdateUser(UserDto productDto);
	}
}
