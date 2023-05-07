using System;
using System.Collections.Generic;

namespace VkIntern.API.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateOnly CreatedDate { get; set; }

    public int UserGroupId { get; set; }

	public int UserStateId { get; set; }

    public virtual UserGroup UserGroup { get; set; } = null!;

    public virtual UserState UserState { get; set; } = null!;
}
