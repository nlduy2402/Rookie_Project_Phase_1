using RetailSystem.Infrastructure.Persistence;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Seed;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Repository;
using RetailSystem.Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
// Cookie (MVC)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});
builder.Services.Configure<JwtSetting>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService,ProductService>();
builder.Services.AddScoped<ICategoryService,CategoryService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRoles(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
