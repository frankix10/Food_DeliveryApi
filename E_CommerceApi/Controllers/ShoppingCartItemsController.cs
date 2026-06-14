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
    public class ShoppingCartItemsController : ControllerBase
    {
        private ApiDbContext _dbContexte;
        public ShoppingCartItemsController(ApiDbContext apiDbContext)
        {
            _dbContexte = apiDbContext;
        }


        [HttpPost("add")]
        public async Task<IActionResult> post([FromBody] ShoppingCardItem shoppingCardItem)
        {
            var existingProduct = await _dbContexte.shoppingCardItems.FirstOrDefaultAsync(p => p.UserId == shoppingCardItem.UserId && p.ProductId == shoppingCardItem.ProductId);
            if (existingProduct != null)
            {
                existingProduct.Qty += shoppingCardItem.Qty;
                existingProduct.TotalAmount = existingProduct.UnitPrice * existingProduct.Qty;
            }
            else
            {
                var product = await _dbContexte.Products.FindAsync(shoppingCardItem.ProductId);
                var cartProduct = new ShoppingCardItem
                {
                    UserId = shoppingCardItem.UserId,
                    ProductId = shoppingCardItem.ProductId,
                    Qty = shoppingCardItem.Qty,
                    UnitPrice = product!.Price,
                    TotalAmount = product.Price * shoppingCardItem.Qty
                };
                await _dbContexte.shoppingCardItems.AddAsync(cartProduct);
            }
            await _dbContexte.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created);

        }


        [HttpPut]
        public async Task<IActionResult> Update([FromQuery] int productId, [FromQuery] string action)
        {
            var userEmail = User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Email)?.Value;
            var user = await _dbContexte.Users.FirstOrDefaultAsync(s => s.Email == userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            var cartItem = await _dbContexte.shoppingCardItems.FirstOrDefaultAsync(p => p.ProductId == productId && p.UserId == user.Id);
            if (cartItem == null)
            {
                return NotFound("not found");
            }
               
            switch (action.ToLower())
            {
                case "increase":
                    cartItem.Qty += 1;
                    break;
                case "dicrease":
                    if (cartItem.Qty > 1)
                    {
                        cartItem.Qty -= 1;
                    }
                    else
                        _dbContexte.shoppingCardItems.Remove(cartItem);
                    break;

                default:
                    return BadRequest("user increase or decrease");

            }
            cartItem.TotalAmount = cartItem.UnitPrice * cartItem.Qty;
            await _dbContexte.SaveChangesAsync();
            return Ok("record updated");

        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userEmail = User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email)?.Value;
            var user = await _dbContexte.Users.FirstOrDefaultAsync(s => s.Email == userEmail);
            if (user == null)
            {
                return NotFound("user not found");
            }

            var userCart = await _dbContexte.shoppingCardItems.Where(s => s.UserId == user.Id)
                 .Include(s => s.Product)
                 .Select(s => new
                 {
                     UserId = s.Id,
                     ProductId = s.ProductId,
                     Qty = s.Qty,
                     UnitePice = s.UnitPrice,
                     TotalAmount = s.TotalAmount,
                     ProductName = s.Product!.Name,
                     ProductImageUrl = s.Product.ImageUrl

                 }).ToListAsync();
            return Ok(userCart);
        }

    }
}
