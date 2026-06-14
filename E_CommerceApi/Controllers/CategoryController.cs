using E_CommerceApi.Data;
using E_CommerceApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private ApiDbContext _dbContext;
        public CategoryController(ApiDbContext apiDbContext)
        {
            _dbContext = apiDbContext;
        }

        // GET ALL CATEGORIES
        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _dbContext.Categories.ToListAsync();
            if (result.Any())
            {
                return Ok(result);
            }
            return NotFound("No Categories Found");
        }

        // POST A NEW CATEGORY
        [HttpPost("AddCategory")]
        public async Task<IActionResult> AddCategory([FromForm] Category category)
        {

            if (category.Image != null)
            {
                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot", guid + ".png");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await category.Image.CopyToAsync(stream);
                }
                category.ImageUrl = filePath.Substring(8);

                await _dbContext.Categories.AddAsync(category);
                await _dbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status201Created);
            }
            return BadRequest("Image not loaded");
        }

        // PUt CAtegory
        [HttpPut("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] Category category)
        {
            var existingCategory = await _dbContext.Categories.FirstOrDefaultAsync(p => p.Id == id);
            if (existingCategory != null)
            {
                if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                {
                    var oldImage = Path.Combine("wwwroot", existingCategory.ImageUrl);
                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }
                if (category.Image != null)
                {
                    var guid = Guid.NewGuid();
                    var filePath = Path.Combine("wwwroot", guid + ".png");
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await category.Image.CopyToAsync(stream);
                    }

                    existingCategory.Name = category.Name;
                    existingCategory.ImageUrl = filePath.Substring(8);

                    await _dbContext.SaveChangesAsync();
                    return Ok("Category updated successfully");
                }
                return NotFound("please insert image");

            }
            return BadRequest("Category not found");
        }

        // Delete CATEGORY
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var existingCategory = await _dbContext.Categories.FirstOrDefaultAsync(u => u.Id == id);
            if (existingCategory!=null)
            {
                if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                {
                    var imageDir = Path.Combine("wwwroot", existingCategory.ImageUrl);
                    if (System.IO.File.Exists(imageDir))
                    {
                        System.IO.File.Delete(imageDir);
                    }

                    _dbContext.Remove(existingCategory);
                    await _dbContext.SaveChangesAsync();
                    return Ok("Category successfull deleted");
                }
            }
            return BadRequest("Invalid ID");
        }
    }
}
