// Ecommerce.Api/Domain/Entities/User.cs
namespace Ecommerce.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string ReferralCode { get; set; } = "";
    public Guid? ReferredByUserId { get; set; }
    public User? ReferredByUser { get; set; }

    // "Admin" أو "User"
    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
