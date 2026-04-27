using Microsoft.EntityFrameworkCore;
using RetailSystem.Infrastructure.Persistence;
using Scalar.AspNetCore;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using Serilog.Events;
using RetailSystem.API.Shared;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Repository;

// Use Serilog for logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting server...");
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.

    builder.Host.UseSerilog();

    // Add DbContext with SQL Server
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
    builder.Services.AddProblemDetails();

    var jwt = builder.Configuration.GetSection("Jwt");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            )
        };
    });

    builder.Services.AddAuthorization();
    builder.Services.AddMemoryCache();


    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IAdminService, AdminService>();


    builder.Services.AddCors(options => {
        options.AddPolicy("AllowAll", builder => builder.WithOrigins("https://localhost:5173").AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });
    // .
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();


    // add Swagger UI
    builder.Services.AddEndpointsApiExplorer();

    var app = builder.Build();

    //Global exception handling
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        //app.UseDeveloperExceptionPage(); 
    }
    else
    {
        app.UseExceptionHandler(); 
    }
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
    app.UseSerilogRequestLogging();

    app.UseCors("AllowAll");
    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Server terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

