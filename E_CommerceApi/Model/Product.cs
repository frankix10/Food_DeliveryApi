using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceApi.Model
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Material { get; set; }
        public string? Gender { get; set; }

        public bool IsBestSelling { get; set; }
        public bool IsTrending { get; set; }


        public int CategoryId { get; set; }
        [JsonIgnore]
        public Category? Category { get; set; }

        [NotMapped,JsonIgnore]
        public IFormFile? Image { get; set; }

        // Collection navigation properties
        [JsonIgnore]
        public ICollection<ShoppingCardItem>? ShoppingCardItems { get; set; }
        [JsonIgnore]
        public ICollection<OrderDetail>? OrderDetails { get; set; }

    }
}
