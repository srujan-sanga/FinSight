namespace IdentityService.Internal.Contracts.DataContracts.Entities;

public sealed class DbUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime DateOfBirth { get; set; }
}