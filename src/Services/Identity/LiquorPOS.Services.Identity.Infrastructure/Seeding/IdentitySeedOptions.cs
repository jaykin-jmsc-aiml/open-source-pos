namespace LiquorPOS.Services.Identity.Infrastructure.Seeding;

public sealed class IdentitySeedOptions
{
    public AdminUserOptions Admin { get; set; } = new();

    public sealed class AdminUserOptions
    {
        public string Email { get; set; } = "admin@liquorpos.local";
        public string FirstName { get; set; } = "System";
        public string LastName { get; set; } = "Administrator";
        public string Password { get; set; } = "ChangeMe!123";
        public string? PhoneNumber { get; set; } = "+10000000000";
    }
}
