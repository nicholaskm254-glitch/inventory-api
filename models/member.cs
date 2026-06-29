namespace InventoryApi.Models
{
    public class Member
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? Role { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;
    }
}