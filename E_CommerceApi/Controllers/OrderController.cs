using E_CommerceApi.Data;
using E_CommerceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_CommerceApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ApiDbContext _dbContext;
        public OrderController(ApiDbContext apiDbContext)
        {
            _dbContext= apiDbContext;
        }

        [HttpPost("post_order")]
        public async Task<IActionResult> AddOrder([FromBody] Order order)
        {
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            var cartItem = await _dbContext.shoppingCardItems.Where(s => s.UserId == order.UserId).ToListAsync();

            order.OrderDate=DateTime.UtcNow;
            order.Status = "pending";
            order.TotalAmount = cartItem.Sum(s => s.TotalAmount);

            foreach (var item in cartItem)
            {
                var orderDetail= new OrderDetail()
                {
                    Qty = item.Qty,
                    UnitPrice = item.UnitPrice,
                    TotalAmount = item.TotalAmount,
                    ProductId = item.ProductId,
                    OrderId=order.Id,
                };
                await _dbContext.OrderDetails.AddAsync(orderDetail);
            }
            await _dbContext.SaveChangesAsync();
            _dbContext.shoppingCardItems.RemoveRange(cartItem);
            await _dbContext.SaveChangesAsync();
            return Ok($"Your order has been place and the order id is : {order.Id}");
        }

        [HttpGet("orders")]
        public async Task<IActionResult> UserOrders()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
          var userOrders=  await _dbContext.Orders.Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.OrderDate)
                .Select(d => new
                {
                    d.Id,
                    d.TotalAmount,
                    d.OrderDate
                }).ToListAsync();
            return Ok(userOrders);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            var productDetail = await _dbContext.OrderDetails.Where(s => s.OrderId == orderId)
                 .Include(s => s.Products)
                 .Select(s => new
                 {
                     Id = s.Id,
                     Qty = s.Qty,
                     TotalAmount = s.TotalAmount,
                     ProductName = s.Products!.Name,
                     PoductImageUrl = s.Products.ImageUrl,
                     ProductPrice = s.Products.Price,
                 }).ToListAsync();
            return Ok(productDetail);
        }
    }
}
