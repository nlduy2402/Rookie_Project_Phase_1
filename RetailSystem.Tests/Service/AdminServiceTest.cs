using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Repository.Interface;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Shared.DTOs;
using RetailSystem.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RetailSystem.Shared.ResponseModels;

namespace RetailSystem.Tests.Service
{
    public class AdminServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IBaseRepository<AdminAccount>> _adminRepoMock;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        private readonly AdminService _service;

        public AdminServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _adminRepoMock = new Mock<IBaseRepository<AdminAccount>>();

            _uowMock.Setup(x => x.AdminAccounts)
                    .Returns(_adminRepoMock.Object);

            var settings = new Dictionary<string, string>
        {
            {"Jwt:Key", "7e32fdb29b08ee39ea6e690e18e6b75f414105fdba2d54880ff8043a5a365c6a9ab5e9d62c2dd1b82d8941b914f45ac707d5899a824551152e3967497357da22"},
            {"Jwt:Issuer", "test"},
            {"Jwt:Audience", "test"}
        };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _cache = new MemoryCache(new MemoryCacheOptions());

            _service = new AdminService(_cache, _config, _uowMock.Object);
        }

        // Test RegisterAsync when username already exists, it should return failure
        [Fact]
        public async Task Register_Should_Fail_When_Username_Exists()
        {
            _adminRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AdminAccount, bool>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new AdminAccount());

            var result = await _service.RegisterAsync(new LoginDTO
            {
                Username = "admin",
                Password = "123"
            });

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Username already exists");
        }

        // Test RegisterAsync when username not exists, it should create account and return success
        [Fact]
        public async Task Register_Should_Success()
        {
            _adminRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<AdminAccount, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((AdminAccount?)null);

            _adminRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<AdminAccount>()))
                .Returns(Task.CompletedTask);

            _adminRepoMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new AdminAccount());

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _service.RegisterAsync(new LoginDTO
            {
                Username = "admin",
                Password = "123"
            });

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }


        // Helper method to create admin with hashed password
        private AdminAccount CreateAdminWithPassword(string password)
        {
            using var hmac = new HMACSHA512();

            var passwordSalt = hmac.Key;
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return new AdminAccount
            {
                Id = 1,
                Username = "admin",
                Role = "Admin",
                PasswordSalt = ByteConvert.ByteArrayToString(passwordSalt),
                PasswordHash = ByteConvert.ByteArrayToString(passwordHash)
            };
        }

        // ================= LOGIN =================


        // Login should fail when user not exist
        [Fact]
        public async Task Login_Should_Fail_When_User_Not_Exist()
        {
            _adminRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<AdminAccount, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync((AdminAccount)null);

            var result = await _service.LoginAsync(new LoginDTO
            {
                Username = "admin",
                Password = "123"
            });

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Login_Should_Fail_When_Wrong_Password()
        {
            var admin = CreateAdminWithPassword("123");

            _adminRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<AdminAccount, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(admin);

            var result = await _service.LoginAsync(new LoginDTO
            {
                Username = "admin",
                Password = "wrong"
            });

            result.IsSuccess.Should().BeFalse();
        }

        // Login should succeed when username and password are correct
        [Fact]
        public async Task Login_Should_Success()
        {
            var admin = CreateAdminWithPassword("123");

            _adminRepoMock
                .Setup(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<AdminAccount, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(admin);

            _adminRepoMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(admin);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _service.LoginAsync(new LoginDTO
            {
                Username = "admin",
                Password = "123"
            });

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        // ================= TOKEN =================

        // Test GenerateToken should return a token string
        [Fact]
        public void GenerateToken_Should_Return_Token()
        {
            var admin = new AdminAccount
            {
                Id = 1,
                Username = "admin",
                Role = "Admin"
            };

            var token = _service.GenerateToken(admin);

            token.Should().NotBeNullOrEmpty();
        }


        // ================= REFRESH TOKEN =================

        // Test SetRefreshToken should update user's refresh token
        [Fact]
        public async Task SetRefreshToken_Should_Update_User()
        {
            var admin = new AdminAccount { Id = 1 };

            _adminRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(admin);

            _uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var token = _service.GenerateRefreshToken();

            await _service.SetRefreshToken(token, admin);

            admin.RefreshToken.Should().NotBeNull();
        }

        // Test SetRefreshToken should not crash when user not found
        [Fact]
        public async Task SetRefreshToken_Should_Not_Crash_When_User_Not_Found()
        {
            _adminRepoMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AdminAccount)null);

            var token = _service.GenerateRefreshToken();

            await _service.SetRefreshToken(token, new AdminAccount { Id = 1 });

            // pass nếu không exception
            true.Should().BeTrue();
        }

    }
}
