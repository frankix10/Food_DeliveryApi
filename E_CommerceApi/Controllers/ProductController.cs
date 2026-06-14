using E_CommerceApi.Data;
using E_CommerceApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private ApiDbContext _dbContext;
        public ProductController(ApiDbContext apiDbContext)
        {
            _dbContext = apiDbContext;
        }

        [HttpGet("Type")]
        public async Task<IActionResult> GetType(string productType, int? productId = null)
        {
            var productQuery = _dbContext.Products.AsQueryable();
            if (productId != null && productType == "category")
            {
                productQuery = productQuery.Where(p => p.CategoryId == productId);
            }
            else if (productId == null && productType == "trending")
            {
                productQuery = productQuery.Where(p => p.IsTrending == true);
            }
            else if (productId == null && productType == "selling")
            {
                productQuery = productQuery.Where(p => p.IsBestSelling == true);
            }
            else
            {
                throw new ArgumentException("Invalid product type. Valid values are 'category', 'trending', or 'selling'.");
            }
            var products = await productQuery.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.ImageUrl,
            }).ToListAsync();
            if (products.Any())
            {
                return Ok(products);
            }
            return NotFound("No products found for the specified type.");


        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchProduct([FromQuery] string? search)
        {
            List<Product> products;
            if (string.IsNullOrWhiteSpace(search))
            {
                products = await _dbContext.Products.Where(p => p.IsBestSelling == true).ToListAsync();
            }
            else
            {
                products = await _dbContext.Products.Where(p =>
            !string.IsNullOrWhiteSpace(p.Name!.ToLower()) && p.Name.ToLower().IndexOf(search.ToLower()) >= 0 ||
            !string.IsNullOrWhiteSpace(p.Description!.ToLower()) && p.Description.ToLower().IndexOf(search.ToLower()) >= 0).ToListAsync();

            }
            return Ok(products);

            //return NotFound("No products found matching the search criteria.");
            //products = await _dbContext.Products.Where(p =>
            //p.Name!.ToLower()) && p.Name.ToLower().Contains(search.ToLower()));.ToListAsync();
            //if (products.Any())
            //{
            //    return Ok(products);
            //}
            //return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _dbContext.Products.ToListAsync();
            if (products.Any())
                return Ok(products);
            return NotFound("No products found.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                return Ok(product);
            }
            return NotFound($"No product found with id {id}");
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] Product product)
        {
            if (product.Image != null)
            {
                var guid = Guid.NewGuid().ToString();
                var filePath = Path.Combine("wwwroot", guid + ".png");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.Image.CopyToAsync(stream);
                }
                product.ImageUrl = filePath.Substring(8);

                await _dbContext.AddAsync(product);
                await _dbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status201Created);
            }
            return BadRequest("please select an image file");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product product)
        {
            var existingProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existingProduct != null)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldImage = Path.Combine("wwwroot", existingProduct.ImageUrl);
                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }

                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot", guid + ".png");
                using (var stram = new FileStream(filePath, FileMode.Create))
                {
                    await product.Image!.CopyToAsync(stram);
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.ImageUrl = filePath.Substring(8);
                existingProduct.Material = product.Material;
                existingProduct.Gender = product.Gender;
                existingProduct.IsBestSelling = product.IsBestSelling;
                existingProduct.IsTrending = product.IsTrending;
                existingProduct.CategoryId = product.CategoryId;

                await _dbContext.SaveChangesAsync();
                return Ok("Product update successfully");

            }
            return BadRequest("something went wrong");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existingProduct = await _dbContext.Products.FirstOrDefaultAsync(u => u.Id == id);
            if (existingProduct != null)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldImage = Path.Combine("wwwroot", existingProduct.ImageUrl);
                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }
                _dbContext.Remove(existingProduct);
                await _dbContext.SaveChangesAsync();
                return Ok("product deleted successfully");
            }
            return BadRequest("wrong Id");
        }
    }
}
