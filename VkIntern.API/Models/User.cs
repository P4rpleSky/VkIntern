using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VkIntern.API.Models;

public partial class User
{
    [Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

    [Required]
    public string Login { get; set; } = null!;

	[Required]
	public string Password { get; set; } = null!;

	[Required]
	public DateOnly CreatedDate { get; set; }

	[Required]
	public int UserGroupId { get; set; }

	[Required]
	public int UserStateId { get; set; }

	[ForeignKey("UserGroupId")]
	public virtual UserGroup UserGroup { get; set; } = null!;

	[ForeignKey("UserStateId")]
	public virtual UserState UserState { get; set; } = null!;
}
