using Moq;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Repository.Interface;
using RetailSystem.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.Service
{
    public class UserServiceTest
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly UserService _service;

        public UserServiceTest()
        {
            _userRepoMock = new Mock<IUserRepository>();

            _service = new UserService(_userRepoMock.Object);
        }
        [Fact]
        public async Task GetAllCustomerAccounts_ShouldReturnData()
        {
            var users = new List<User>
            {
                new User
                {
                    Id = "1",
                    UserName = "test",
                    FullName = "Test User",
                    Email = "test@gmail.com"
                }
            };

            _userRepoMock.Setup(x => x.GetAllCustomersAsync())
                .ReturnsAsync(users);

            var result = await _service.GetAllCustomerAccountsAsync();

            Assert.True(result.IsSuccess);
            Assert.Single(result.Data);
        }
    }
}
