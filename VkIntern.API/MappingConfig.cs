using AutoMapper;
using VkIntern.API.Models;
using VkIntern.API.Models.Dtos;

namespace VkIntern.API
{
	public class MappingConfig
	{
		public static MapperConfiguration RegisterMaps()
		{
			var mappingConfig = new MapperConfiguration(config =>
			{
				config.CreateMap<UserDto, User>().ReverseMap();
				config.CreateMap<User, UserSummaryDto>()
					.AfterMap((user, userSummaryDto) =>
					{
						userSummaryDto.UserStateCode = user.UserState.Code;
						userSummaryDto.UserStateDescription = user.UserState.Description;
						userSummaryDto.UserStateCode = user.UserState.Code;
						userSummaryDto.UserStateDescription = user.UserState.Description;
					});
				//config.CreateMap<UserSummaryDto, UserDto>();
			});
			return mappingConfig;
		}
	}
}
