using System.Text.Json.Serialization;

namespace E_CommerceApi.Model
{
    public class ShoppingCardItem
    {
        public int Id { get; set; }
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; }
        public decimal TotalAmount { get; set; }

        public int UserId { get; set; }
        public int ProductId { get; set; }
        


        [JsonIgnore]
        public Product? Product { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
    }
}
