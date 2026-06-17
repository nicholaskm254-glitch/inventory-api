namespace InventoryApi.Models
{
    public class Member
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? Role { get; set; }
    }
}