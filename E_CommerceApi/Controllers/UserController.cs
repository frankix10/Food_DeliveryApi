using Azure.Messaging;
using E_CommerceApi.Data;
using E_CommerceApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace E_CommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ApiDbContext _dbContext;
        private IConfiguration _configuration;
        public UserController(ApiDbContext apiDbContext, IConfiguration configuration)
        {
            _dbContext = apiDbContext;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already exists.");
            }

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);

            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return Created("", "Account created successfully");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest user)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, user.Password);

            if (result != PasswordVerificationResult.Success)
            {
                return BadRequest("Invalid password.");
            }

            var security = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credential = new SigningCredentials(security, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, existingUser.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credential
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = jwt,
                token_type = "Bearer",
                user_id=existingUser.Id,
                user_image=existingUser.ImageUrl,
                user_name =existingUser.Name,
                message = "Login successful"
            });

            
        }

        [HttpPost("request")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(p => p.Email == email);
            if (user==null)
            {
                return NotFound("Email not found.");
            }
            user.Token = GenerateToken();
            user.ExpiryTime = DateTime.Now.AddMinutes(1);

            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                token = user.Token,
                MessageContent=$"you may replace your password, {user.Name}"
            });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword(string token, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Token == token);
            if (user == null)
                return BadRequest("Invalid tokent");
            if(user.ExpiryTime <= DateTime.Now)
                return BadRequest("Token expired");

            var passwordHash = new PasswordHasher<User>();
            user.Password = passwordHash.HashPassword(user, password);

            user.Token = null;
            user.ExpiryTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return Ok("Password updated successfully");
        }

        [HttpPost("upload_image")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var user =await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return BadRequest("user not found");
            
            if (image != null)
            {
                var imageDir = Path.Combine("wwwroot", "UserImages");
                Directory.CreateDirectory(imageDir);
                if (!string.IsNullOrWhiteSpace(user.ImageUrl))
                {
                    var existingImage = Path.GetFileName(user.ImageUrl);
                    if (existingImage != null)
                    {
                        var fullImagePath = Path.Combine(imageDir, existingImage);
                        if (System.IO.File.Exists(fullImagePath))
                        {
                            System.IO.File.Delete(fullImagePath);
                        }
                    }
                       
                }

                var guid = Guid.NewGuid().ToString()+"_"+image.FileName;
                var filePath = Path.Combine(imageDir, guid);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                user.ImageUrl = "/UserImages/" + guid;
                await _dbContext.SaveChangesAsync();
                return Ok(new
                {
                    imageUrl = user.ImageUrl,
                    MessageContent = $"Image uploaded successfully, {user.Name}"
                });
            }
            return BadRequest("No image provided");
        }



        // Generate Token
        private string? GenerateToken()
        {
           return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }

        // Login raquest
        public class LoginRequest
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
        }

       

    }
}
