using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Project3.Services; // Thêm namespace của JwtService nếu cần
using Project3.Data; // Namespace của ApplicationDbContext
using Project3.Models; // Namespace của LoginModel
using Project3.Controllers; // Namespace của AuthController nếu cần

var builder = WebApplication.CreateBuilder(args);

// Đọc cấu hình SMTP từ appsettings.json
var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
builder.Services.AddSingleton(smtpSettings);
builder.Services.AddScoped<EmailService>();



// Cấu hình DbContext cho Entity Framework
builder.Services.Configure<ImgurSettings>(builder.Configuration.GetSection("Imgur"));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 403))));



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };

        // Lấy token từ cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("jwt"))
                {
                    context.Token = context.Request.Cookies["jwt"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllersWithViews();

// Đăng ký dịch vụ JwtService (thêm từ phần trước nếu cần)
builder.Services.AddSingleton(new JwtService(
    builder.Configuration["Jwt:SecretKey"],
    builder.Configuration["Jwt:Issuer"]
));

var app = builder.Build();

// Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}
// app.UseHttpsRedirection();
app.UseRouting();

// Sử dụng các middleware bảo mật như xác thực và phân quyền
app.UseAuthentication(); // Phải có dòng này để kích hoạt xác thực
app.UseAuthorization();  // Phải có dòng này để kiểm tra quyền

// Đảm bảo sử dụng tài nguyên tĩnh như hình ảnh, JavaScript và CSS
app.UseStaticFiles();

// Cấu hình các route cho các controller và các action tương ứng
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    "admin",
    "admin/{action=Admin}/{id?}",
    new { controller = "Admin", action = "Admin" }
);

// Cấu hình route cho AuthController
app.MapControllerRoute(
    "auth",
    "auth/{action=Login}/{id?}",
    new { controller = "Auth", action = "Login" }
);

// Chạy ứng dụng
app.Run();
