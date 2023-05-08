using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VkIntern.API.Models;

public partial class UserState
{
    [Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

    [Required]
    public string Code { get; set; } = null!;

	[Required]
	public string? Description { get; set; }
}
