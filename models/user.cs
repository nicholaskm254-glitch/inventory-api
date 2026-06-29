namespace InventoryApi.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public int CompanyId { get; set; }

    public Company? Company { get; set; }

    public bool IsDeleted { get; set; } = false;
   public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}