using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace VkIntern.API.Models;

public partial class UserGroup
{
	public int Id { get; set; }

	public string Code { get; set; } = null!;

	public string? Description { get; set; }

	public virtual ICollection<User> Users { get; set; } = new List<User>();
}
