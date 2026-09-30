using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public DateTimeOffset? LastConfirmationEmailAt { get; set; }
    public DateTimeOffset? LastPasswordResetEmailAt { get; set; }
}
