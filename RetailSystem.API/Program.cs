using Microsoft.EntityFrameworkCore;
using RetailSystem.Infrastructure.Persistence;
using Scalar.AspNetCore;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
builder.Services.Configure<JwtSetting>(
    builder.Configuration.GetSection("Jwt"));
//builder.Services.AddAuthentication()
//    .AddJwtBearer(options =>
//    {
//        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidIssuer = jwtSettings.Issuer,
//            ValidAudience = jwtSettings.Audience,
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(jwtSettings.Secret))
//        };
//    });


builder.Services.AddScoped<ICategoryService, CategoryService> ();
builder.Services.AddScoped<IProductService,ProductService>();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
// .
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// add Swagger UI
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("RetailSystem API")
               .WithTheme(ScalarTheme.Mars)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
