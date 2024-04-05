﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mutualify.Database.Models;

public class Token
{
    [Key]
    [ForeignKey("User")]
    public required int UserId { get; set; }
    public required string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public required DateTime ExpiresOn { get; set; }

    public User User { get; set; } = null!;
}
