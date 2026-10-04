using System;
using System.Collections.Generic;

namespace CinemaBooking.Identity.Api.Entities;

public partial class UserAccount
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;
}
