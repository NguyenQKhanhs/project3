using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Project3.Data; // Namespace của ApplicationDbContext
using Project3.Models; // Namespace của LoginModel

namespace Project3.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }


        // View: Hiển thị form đăng nhập
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        // // View: Xử lý form đăng nhập
        // [HttpPost("login")]
        // public IActionResult Login(string username, string password)
        // {
        //     // Tìm user trong database với Username và PasswordHash
        //     var account = _dbContext.Accounts.FirstOrDefault(a => a.Username == username && a.Password == password);
        //
        //     if (account != null)
        //     {
        //         // Tạo JWT
        //         var claims = new[] {
        //             new Claim(ClaimTypes.Name, account.Username),
        //         };
        //
        //         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        //         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //
        //         var token = new JwtSecurityToken(
        //             issuer: _configuration["Jwt:Issuer"],
        //             audience: _configuration["Jwt:Audience"],
        //             claims: claims,
        //             expires: DateTime.Now.AddDays(1),
        //             signingCredentials: creds
        //         );
        //
        //         // Lưu token vào cookie
        //         Response.Cookies.Append("jwt", new JwtSecurityTokenHandler().WriteToken(token));
        //
        //         // Chuyển hướng sau khi đăng nhập thành công
        //         return RedirectToAction("Index", "Admin");
        //     }
        //
        //     // Trả lỗi nếu thông tin không đúng
        //     TempData["Error"] = "Invalid username or password";
        //     return RedirectToAction("Login", "Admin");
        // }

        // [HttpPost("login")]
        // public IActionResult Login(string username, string password)
        // {
        //     var account = _dbContext.Accounts.FirstOrDefault(a => a.Username == username);
        //     if (account != null)
        //     {
        //         var passwordHasher = new PasswordHasher<Account>();
        //         var result = passwordHasher.VerifyHashedPassword(account, account.Password, password);
        //         if (result == PasswordVerificationResult.Success)
        //         {
        //             var claims = new[] { new Claim(ClaimTypes.Name, account.Username) };
        //             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        //             var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //             var token = new JwtSecurityToken(
        //                 issuer: _configuration["Jwt:Issuer"],
        //                 audience: _configuration["Jwt:Audience"],
        //                 claims: claims,
        //                 expires: DateTime.Now.AddDays(1),
        //                 signingCredentials: creds
        //             );
        //
        //             Response.Cookies.Append("jwt", new JwtSecurityTokenHandler().WriteToken(token));
        //             return RedirectToAction("Index", "Admin");
        //         }
        //     }
        //
        //     TempData["Error"] = "Invalid username or password";
        //     return RedirectToAction("Login", "Admin");
        // }

        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            var account = _dbContext.Accounts.FirstOrDefault(a => a.Username == username);
            if (account != null)
            {
                var passwordHasher = new PasswordHasher<Account>();
                var result = passwordHasher.VerifyHashedPassword(account, account.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, account.Username) };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: creds
                    );

                    Response.Cookies.Append("jwt", new JwtSecurityTokenHandler().WriteToken(token));
                    return RedirectToAction("Index", "Admin");
                }
            }

            TempData["Error"] = "Invalid username or password";
            return RedirectToAction("Login", "Admin");
        }


        // View: Đăng xuất
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // Xóa JWT khỏi cookie
            Response.Cookies.Delete("jwt");

            // Chuyển hướng về trang login
            return RedirectToAction("Login", "Admin");
        }
    }
}
