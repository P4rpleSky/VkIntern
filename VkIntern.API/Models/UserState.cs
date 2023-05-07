using System;
using System.Collections.Generic;

namespace VkIntern.API.Models;

public partial class UserState
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
