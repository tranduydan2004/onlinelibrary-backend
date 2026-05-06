using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; // Dùng cho cấu hình Swagger 
using OnlineLibrary.Application.Services;
using OnlineLibrary.Infrastructure.Services;
using Microsoft.Extensions.FileProviders;
using OnlineLibrary.Application.Common;
using OnlineLibrary.Application.Interfaces.Repositories;
using OnlineLibrary.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; // Lấy IConfiguration

// Load ENV
builder.Configuration.AddEnvironmentVariables();

// --- 1. ĐĂNG KÝ DỊCH VỤ VÀO CONTAINER (DI) ---

// 1.1 Lấy chuỗi kết nối từ Configuration (hỗ trợ cả biến môi trường từ docker-compose)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"];

// Nếu không có, dự phòng bằng cách tự build (hữu ích khi chạy local)
if (string.IsNullOrEmpty(connectionString))
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? builder.Configuration["DB_HOST"];
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? builder.Configuration["DB_PORT"];
    var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? builder.Configuration["DB_NAME"];
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? builder.Configuration["DB_USER"];
    var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? builder.Configuration["DB_PASSWORD"];
    if (!string.IsNullOrEmpty(dbHost))
    {
        connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass}";
    }
}
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// JWT
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_KEY")))
{
    builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY");
    builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER");
    builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
}

// SMTP
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SMTP_USER")))
{
    builder.Configuration["Smtp:User"] = Environment.GetEnvironmentVariable("SMTP_USER");
    builder.Configuration["Smtp:Password"] = Environment.GetEnvironmentVariable("SMTP_PASS");
    builder.Configuration["Smtp:From"] = Environment.GetEnvironmentVariable("SMTP_FROM");
}

// Add services to the container.

builder.Services.AddControllers();
// 1.2. Đăng ký ApplicationDbContext với Dependency Injection
// Cần cài đặt NuGet package: Npgsql.EntityFrameworkCore.PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Các dịch vụ khác (Controllers, Services, Authentication, ...)
// 1.3 Đăng ký các Repository và Service nghiệp vụ (Dependency Injection)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IBookAdminService, BookAdminService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();
builder.Services.AddScoped<ILoanAdminService, LoanAdminService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddHttpClient<IWebhookService, N8nWebhookService>();

// 1.4 Cấu hình xác thực JWT (Authentication)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });

// 1.5 Cấu hình CORS (Rất quan trọng để React có thể gọi API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            // Thay đổi http://localhost:3000 thành địa chỉ của Frontend React
            policy
                .AllowAnyOrigin()
                //.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// 1.7 Cấu hình Swagger/OpenAPI (để test API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Online Library API", Version = "v1" });

    // Cấu hình để Swagger UI có thể gửi token JWT khi test API
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer' [khoảng trắng] rồi dán token của bạn vào.\n\n Ví dụ: 'Bearer 12345abcdef"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// --- 2. Cấu hình HTTP Request Pipeline (Middleware) ---
// Configure the HTTP request pipeline.
// Chỉ chạy Swagger trong môi trường Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Library API v1"));
}

// app.UseHttpsRedirection();

// Lấy webRootPath
var webRootPath = app.Environment.WebRootPath
                  ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

// Đảm bảo thư mục uploads/covers tồn tại
var coversPath = Path.Combine(webRootPath, "uploads", "covers");
Directory.CreateDirectory(coversPath);

// 1. Serve toàn bộ wwwroot
app.UseStaticFiles();

// 2. Serve riêng thư mục uploads/covers tại đúng đường dâxn /uploads/covers
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(coversPath),
    RequestPath = "/uploads/covers"
});

// Migration DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Kích hoạt CORS (Phải đứng trước UseAuthorization)
app.UseCors("AllowReactApp");

// Kích hoạt Authentication (Phải đứng trước Authorization)
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
