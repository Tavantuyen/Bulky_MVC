using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BulkyBook.Utility;
using Stripe;
using BulkyBook.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Khai báo sử dụng dependency injection database
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//tiêm StripSettings.cs vào Stripe trong file appsetting.json
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));




//options.SignIn.RequireConfirmedAccount = true xác nhận email khi login
//AddEntityFrameworkStores<ApplicationDbContext>(); thêm bảng thực thể manager user
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();

//khai báo tùy chọn dùng AddIdentity và thêm mã thông báo AddDefaultTokenProviders() của nhà cung cấp vì đây là AddIdentity tùy chọn
builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//phải khai báo sau AddIdentity và cấu hình đường đi
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

//cấu hình xác thực facebook
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "502909925401460";
    options.AppSecret = "7a35a51ef28b9b78f43d80409d113c9c";

});

//khai báo bộ nhớ phân tán
builder.Services.AddDistributedMemoryCache();
//khái báo phiên
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//khai báo sử dụng sopce dbInitializer
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

//khai báo sử dụng razor page
builder.Services.AddRazorPages();

// Khai báo sử dụng  dependency injection bằng  interface
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//khai báo sử dụng email sender
builder.Services.AddScoped<IEmailSender,EmailSender>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//
app.UseHttpsRedirection();
//app.UseStaticFiles(); dùng để khai báo sử dụng các tệp tính trong wwwroot
app.UseStaticFiles();

//cấu hình stripe
StripeConfiguration.ApiKey=builder.Configuration.GetSection("Stripe:Secretkey").Get<string>();

//app.UseRouting(); thêm định tuyến để điều hướng
app.UseRouting();

//xác thực trước khi ủy quyền app.UseAuthorization();
app.UseAuthentication();



//app.UseAuthorization();  cho phép quyền truy cập vào trang web dựa trên vai trò  và phải xác thực app.UseAuthentication(); trước đã
app.UseAuthorization();

//thêm phiên sau ủy quyền UseAuthorization
app.UseSession();

//seed database
SeedDatabase();

// định tuyến map razor page
app.MapRazorPages();

//khai báo cách thức định tuyến như thế nào
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer=scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}