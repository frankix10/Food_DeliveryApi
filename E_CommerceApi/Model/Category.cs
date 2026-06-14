using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceApi.Model
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }

        [NotMapped, JsonIgnore]
        public IFormFile? Image { get; set; }
        [JsonIgnore]
        public ICollection<Product>? Products { get; set; }
    }
}
