using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkIntern.API.Models.Dtos;
using VkIntern.API.Repository;

namespace VkIntern.API.Controllers
{
	[Route("api/users")]
	public class UserAPIController : Controller
	{
		protected ResponseDto _response;
		private IUserRepository _userRepository;

		public UserAPIController(IUserRepository userRepository)
		{
			_userRepository = userRepository;
			this._response = new ResponseDto();
		}

		[HttpGet]
		public async Task<ResponseDto> Get()
		{
			try
			{
				var productDtos = await _userRepository.GetUsersInfoAsync();
				_response.Result = productDtos;
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.ToString());
			}
			return _response;
		}

		[HttpGet("{limit:int}/{offset:int}")]
		public async Task<ResponseDto> Get(int limit, int offset)
		{
			try
			{
				var productDtos = await _userRepository.GetUsersInfoAsync(limit, offset);
				_response.Result = productDtos;
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.ToString());
			}
			return _response;
		}

		[HttpGet("{id:int}")]
		public async Task<ResponseDto> Get(int id)
		{
			try
			{
				var productDto = await _userRepository.GetUserInfoByIdAsync(id);
				_response.Result = productDto;
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.ToString());
			}
			return _response;
		}

		[HttpPost]
		public async Task<ResponseDto> Post([FromBody] UserDto userDto)
		{
			try
			{
				var model = await _userRepository.CreateUserAsync(userDto);
				_response.Result = model;
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.ToString());
			}
			return _response;
		}

		[HttpDelete("{id:int}")]
		public async Task<ResponseDto> Delete(int id)
		{
			try
			{
				var isSuccess = await _userRepository.DeleteUserAsync(id);
				_response.Result = isSuccess;
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.ToString());
			}
			return _response;
		}
	}
}
