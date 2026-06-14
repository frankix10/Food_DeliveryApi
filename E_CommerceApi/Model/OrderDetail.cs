using System.Text.Json.Serialization;

namespace E_CommerceApi.Model
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; }
        public decimal TotalAmount { get; set; }

        public int ProductId { get; set; }
        public int OrderId { get; set; }


        [JsonIgnore]
        public Product? Products { get; set; }
        [JsonIgnore]
        public Order? Order { get; set; }
    }
}
