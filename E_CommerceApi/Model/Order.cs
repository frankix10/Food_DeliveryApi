using System.Text.Json.Serialization;

namespace E_CommerceApi.Model
{
    public class Order
    {
        public int Id { get; set; }
        public string? Adress { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }

        public int UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }


    }
}
