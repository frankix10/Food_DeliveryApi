using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceApi.Model
{
    public class User
    {
        public int Id { get; set; }
        [Required, MinLength(4)]
        public string? Name { get; set; }
        [Required, EmailAddress]
        public string? Email { get; set; }
        [Required, MinLength(4)]
        public string? Password { get; set; }
        public string? ImageUrl { get; set; }
        public string? Role { get; set; } = "User";

        // Token generation
        public DateTime ExpiryTime { get; set; }
        public string? Token { get; set; }

        [NotMapped, JsonIgnore]
        public IFormFile? Image { get; set; }

        // Collection Navigations
        public ICollection<ShoppingCardItem>? shoppingCardItems { get; set; }
        public ICollection<Order>? Order { get; set; }
    }
}
