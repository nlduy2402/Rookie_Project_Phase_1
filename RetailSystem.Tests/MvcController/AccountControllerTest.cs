using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RetailSystem.CustomerSite.Controllers;
using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.MvcController
{
    public class AccountControllerTest
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly AccountController _controller;

        public AccountControllerTest()
        {
            // Mock UserManager
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            // Mock SignInManager
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);

            _controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
        }

        // 
        [Fact]
        public async Task Login_Post_ReturnsRedirect_WhenLoginSucceeds()
        {
            // Arrange
            var model = new LoginDTO { Username = "testuser", Password = "Password123" };

            _mockSignInManager
                .Setup(x => x.PasswordSignInAsync(model.Username, model.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_Post_ReturnsViewWithError_WhenLoginFails()
        {
            // Arrange
            var model = new LoginDTO { Username = "wronguser", Password = "wrongpassword" };

            _mockSignInManager
                .Setup(x => x.PasswordSignInAsync(model.Username, model.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal("Invalid Username or Password", _controller.ModelState[""]?.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Register_Post_ReturnsRedirect_WhenRegistrationSucceeds()
        {
            // Arrange
            var model = new RegisterDTO { Username = "newuser", Email = "test@test.com", Password = "Password123" };

            _mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager
                .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Customer"))
                .ReturnsAsync(IdentityResult.Success);

            _mockSignInManager
                .Setup(x => x.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Login_Post_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new LoginDTO { Username = "", Password = "" }; 
            _controller.ModelState.AddModelError("Username", "Required"); 

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model); 
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task Register_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new RegisterDTO { Username = "user" };
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            // Verify: Ensure UserManager.CreateAsync NOT called
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Register_Post_AddsErrorsToModelState_WhenIdentityFails()
        {
            // Arrange
            var model = new RegisterDTO { Username = "existinguser", Email = "test@test.com", Password = "123" };
            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Description = "Username 'existinguser' is already taken." }
            };

            _mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Username 'existinguser' is already taken.",
                _controller.ModelState[""]?.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Register_Post_ReturnsViewWithIdentityErrors_WhenPasswordIsWeak()
        {
            // 1. Arrange
            var model = new RegisterDTO
            {
                Username = "testuser",
                Email = "test@gmail.com",
                Password = "123" 
            };

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 6 characters." },
                new IdentityError { Code = "PasswordRequiresNonAlphanumeric", Description = "Passwords must have at least one non alphanumeric character." },
                new IdentityError { Code = "PasswordRequiresLower", Description = "Passwords must have at least one lowercase ('a'-'z')." },
                new IdentityError { Code = "PasswordRequiresUpper", Description = "Passwords must have at least one uppercase ('A'-'Z')." }
            };

            _mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // 2. Act
            var result = await _controller.Register(model);

            // 3. Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            Assert.Equal(4, _controller.ModelState.ErrorCount);

            var errors = _controller.ModelState[""]?.Errors;
            Assert.Contains(errors, e => e.ErrorMessage.Contains("at least 6 characters"));
            Assert.Contains(errors, e => e.ErrorMessage.Contains("non alphanumeric character"));
        }
    }
}
