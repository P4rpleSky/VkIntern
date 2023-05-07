namespace VkIntern.API.Models.Dtos
{
    public class UserDto
    {
		public string? Login { get; set; }

		public string? Password { get; set; }

		public string UserGroupCode { get; set; } = null!;
	}
}
