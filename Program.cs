using ABC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.RegularExpressions;
using ABC.Data;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

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
            var hostParts = host.Split(':');
            var mainHost = hostParts[0];
            var port = hostParts.Length > 1 ? hostParts[1] : "5432"; // Port mặc định PostgreSQL
            
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
            
            // Cấu hình bảng migrations history
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
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
            var logger = services.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Đang kiểm tra kết nối tới PostgreSQL...");
            
            // Kiểm tra xem database có tồn tại không
            var canConnect = context.Database.CanConnect();
            logger.LogInformation("Kết nối tới database: {CanConnect}", canConnect);
            
            if (canConnect)
            {
                logger.LogInformation("Tạo schema nếu cần...");
                
                // Tạo các bảng cơ bản (quyen, taikhoan) một cách thủ công
                var conn = context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();
                
                using var cmd = conn.CreateCommand();
                
                // Kiểm tra bảng taikhoan đã tồn tại chưa
                cmd.CommandText = "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'taikhoan')";
                var taikhoanExists = (bool)cmd.ExecuteScalar();
                
                if (!taikhoanExists)
                {
                    logger.LogInformation("Tạo bảng taikhoan và quyen...");
                    
                    // Tạo bảng quyen trước
                    using var createQuyenCmd = conn.CreateCommand();
                    createQuyenCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS quyen (
                            maquyen INTEGER PRIMARY KEY,
                            tenquyen VARCHAR(50) NOT NULL
                        )";
                    createQuyenCmd.ExecuteNonQuery();
                    
                    // Tạo bảng taikhoan
                    using var createTaikhoanCmd = conn.CreateCommand();
                    createTaikhoanCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS taikhoan (
                            matk SERIAL PRIMARY KEY,
                            taikhoan VARCHAR(50) UNIQUE NOT NULL,
                            matkhau VARCHAR(50),
                            maquyen INTEGER REFERENCES quyen(maquyen)
                        )";
                    createTaikhoanCmd.ExecuteNonQuery();
                    
                    // Thêm dữ liệu mẫu
                    using var insertQuyenCmd = conn.CreateCommand();
                    insertQuyenCmd.CommandText = @"
                        INSERT INTO quyen (maquyen, tenquyen) VALUES 
                        (1, 'Admin'), 
                        (2, 'Giảng viên'), 
                        (3, 'Sinh viên')
                        ON CONFLICT (maquyen) DO NOTHING";
                    insertQuyenCmd.ExecuteNonQuery();
                    
                    using var insertAdminCmd = conn.CreateCommand();
                    insertAdminCmd.CommandText = @"
                        INSERT INTO taikhoan (taikhoan, matkhau, maquyen) VALUES 
                        ('admin1', '123456', 1)
                        ON CONFLICT DO NOTHING";
                    insertAdminCmd.ExecuteNonQuery();
                    
                    logger.LogInformation("Đã tạo bảng và dữ liệu mẫu thành công");
                }
                else
                {
                    logger.LogInformation("Bảng taikhoan đã tồn tại");
                    
                    // Kiểm tra và in ra số lượng tài khoản
                    cmd.CommandText = "SELECT COUNT(*) FROM taikhoan";
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    logger.LogInformation("Số lượng tài khoản: {Count}", count);
                }
                
                // Đảm bảo có dữ liệu cơ bản
                context.EnsureSeeded();
            }
            else
            {
                logger.LogWarning("Không thể kết nối tới PostgreSQL database!");
            }
            
            logger.LogInformation("Hoàn tất quá trình khởi tạo database");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Lỗi không thể khắc phục khi thiết lập database.");
        }
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();