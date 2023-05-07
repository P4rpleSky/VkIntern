namespace VkIntern.API.Models.Dtos
{
	public class UserSummaryDto
	{
		public int Id { get; set; }

		public string Login { get; set; } = null!;

		public string Password { get; set; } = null!;

		public DateOnly CreatedDate { get; set; }

		public int UserGroupId { get; set; }

		public string UserGroupCode { get; set; } = null!;

		public string? UserGroupDescription { get; set; }

		public int UserStateId { get; set; }

		public string UserStateCode { get; set; } = null!;

		public string? UserStateDescription { get; set; }
	}
}
