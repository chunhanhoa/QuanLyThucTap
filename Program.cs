using ABC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Thay đổi cấu hình dịch vụ DbContext - Sửa cách đăng ký để không xung đột với OnConfiguring
if (builder.Environment.IsProduction())
{
    // Lấy thông tin connection string từ cấu hình
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("CẢNH BÁO: Connection string 'DefaultConnection' không được tìm thấy!");
        throw new InvalidOperationException("Connection string 'DefaultConnection' không được cấu hình cho môi trường Production.");
    }

    // Chuyển đổi định dạng connection string URL sang key-value nếu cần
    if (connectionString.StartsWith("postgresql://"))
    {
        // Phân tích cú pháp URL PostgreSQL
        var regex = new Regex(@"postgresql:\/\/(?<username>[^:]+):(?<password>.+)@(?<host>[^\/]+)\/(?<database>.+)\/?$");
        var match = regex.Match(connectionString);

        if (match.Success)
        {
            var username = match.Groups["username"].Value;
            var password = match.Groups["password"].Value;
            var host = match.Groups["host"].Value;
            var database = match.Groups["database"].Value.TrimEnd('/');
            
            // Tách host và port nếu có
            var hostParts = host.Split('.');
            var mainHost = host;
            var port = "5432"; // Port mặc định PostgreSQL
            
            // Chuyển đổi sang định dạng key-value
            connectionString = $"Host={mainHost};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
            Console.WriteLine("Đã chuyển đổi connection string từ định dạng URL sang định dạng key-value.");
        }
        else
        {
            Console.WriteLine("CẢNH BÁO: Không thể phân tích cú pháp connection string URL PostgreSQL!");
        }
    }

    // In thông tin kết nối (không hiển thị mật khẩu)
    var sanitizedConnectionString = connectionString;
    if (connectionString.Contains("Password="))
    {
        var passwordStart = connectionString.IndexOf("Password=");
        var passwordEnd = connectionString.IndexOf(";", passwordStart);
        if (passwordEnd == -1) passwordEnd = connectionString.Length;
        
        sanitizedConnectionString = connectionString.Substring(0, passwordStart) + 
                                   "Password=*****" + 
                                   (passwordEnd < connectionString.Length ? connectionString.Substring(passwordEnd) : "");
    }
    Console.WriteLine($"Kết nối PostgreSQL: {sanitizedConnectionString}");

    // Đăng ký DbContext với PostgreSQL
    builder.Services.AddDbContext<QlpcthucTapContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        });
        
        // Bật chi tiết logging để gỡ lỗi
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    });
}
else
{
    // Trong môi trường development, để OnConfiguring xử lý
    // nên không đăng ký thêm ở đây để tránh xung đột
}

// Thêm Authentication với Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// Thêm Authorization
builder.Services.AddAuthorization();

// Thêm Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

if (builder.Environment.IsProduction())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443;
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Tắt HSTS để tránh redirect loop
    // app.UseHsts();
}

// Tắt HTTPS redirection để tránh redirect loop trong production
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Đảm bảo favicon không bị cache
        if (ctx.File.Name.Equals("favicon.ico"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
            ctx.Context.Response.Headers.Append("Expires", "-1");
        }
    }
});
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders();

    // Sửa phần migration - thêm try-catch và giảm số lượng retry
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<QlpcthucTapContext>();
            
            // Thêm thời gian chờ và số lần thử lại
            var retryCount = 5;
            while (retryCount > 0)
            {
                try
                {
                    context.Database.Migrate();
                    break; // Thoát khỏi vòng lặp nếu migration thành công
                }
                catch (Exception ex)
                {
                    retryCount--;
                    if (retryCount == 0)
                    {
                        throw; // Nếu đã hết số lần thử, ném ngoại lệ
                    }
                    
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Lỗi khi migrate database. Thử lại sau 5 giây. Còn {RetryCount} lần thử.", retryCount);
                    
                    // Đợi 5 giây trước khi thử lại
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Lỗi không thể khắc phục khi migrate database.");
        }
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();