using Microsoft.AspNetCore.Mvc;
using Moq;
using RetailSystem.CustomerSite.Controllers;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services.Interfaces;
using RetailSystem.Infrastructure.Services;
using RetailSystem.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailSystem.Shared.ResponseModels;
using FluentAssertions;

namespace RetailSystem.Tests.MvcController
{
    public class ProductControllerTest
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IReviewService> _reviewServiceMock;

        private readonly ProductController _controller;

        public ProductControllerTest()
        {
            _productServiceMock = new Mock<IProductService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _reviewServiceMock = new Mock<IReviewService>();

            _controller = new ProductController(
                _productServiceMock.Object,
                _categoryServiceMock.Object,
                _reviewServiceMock.Object
            );
        }

        #region Index Tests
        [Fact]
        public async Task Index_WhenServiceFails_ShouldReturnBadRequest()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetPagedAsync(1, 8))
                .ReturnsAsync(new ServiceResult<PageResult<Product>>
                {
                    IsSuccess = false,
                    Message = "Error"
                });

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;

            badRequest.Value.Should().Be("Error");
        }

        [Fact]
        public async Task Index_WhenServiceSucceeds_ShouldReturnViewResult()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 1000
            }
        };

            var pagedResult = new PageResult<Product>
            {
                Items = products,
                Page = 1,
            };

            _productServiceMock
                .Setup(x => x.GetPagedAsync(1, 8))
                .ReturnsAsync(new ServiceResult<PageResult<Product>>
                {
                    IsSuccess = true,
                    Data = pagedResult
                });

            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;

            viewResult.Model.Should().BeOfType<ProductIndexViewModel>();
        }

        [Fact]
        public async Task Index_ShouldMapProductsToCardViewModel()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 1000
            }
        };

            var pagedResult = new PageResult<Product>
            {
                Items = products,
                Page = 1,
            };

            _productServiceMock
                .Setup(x => x.GetPagedAsync(1, 8))
                .ReturnsAsync(new ServiceResult<PageResult<Product>>
                {
                    IsSuccess = true,
                    Data = pagedResult
                });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as ProductIndexViewModel;

            model.Items.Should().HaveCount(1);

            model.Items.First().Name.Should().Be("Laptop");
        }

        [Fact]
        public async Task Index_ShouldSetCorrectPaginationData()
        {
            // Arrange
            var pagedResult = new PageResult<Product>
            {
                Items = new List<Product>(),
                Page = 2,
                PageSize = 8,
                TotalCount = 80
            };

            _productServiceMock
                .Setup(x => x.GetPagedAsync(2, 8))
                .ReturnsAsync(new ServiceResult<PageResult<Product>>
                {
                    IsSuccess = true,
                    Data = pagedResult
                });

            // Act
            var result = await _controller.Index(2);

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as ProductIndexViewModel;

            model.Page.Should().Be(2);

            model.TotalPages.Should().Be(10);
        }

        [Fact]
        public async Task Index_ShouldCallGetPagedAsyncWithCorrectParameters()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetPagedAsync(3, 8))
                .ReturnsAsync(new ServiceResult<PageResult<Product>>
                {
                    IsSuccess = true,
                    Data = new PageResult<Product>
                    {
                        Items = new List<Product>(),
                        Page = 3,
                    }
                });

            // Act
            await _controller.Index(3);

            // Assert
            _productServiceMock.Verify(
                x => x.GetPagedAsync(3, 8),
                Times.Once
            );
        }

        [Fact]
        public async Task Index_WhenItemsEmpty_ShouldReturnEmptyViewModel()
        {
            // Arrange
            var pagedResult = new PageResult<Product>
            {
                Items = new List<Product>(),
                Page = 1
                
            };

            _productServiceMock
                .Setup(x => x.GetPagedAsync(1, 8))
                .ReturnsAsync(new ServiceResult<PageResult<Product>>
                {
                    IsSuccess = true,
                    Data = pagedResult
                });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as ProductIndexViewModel;

            model.Items.Should().BeEmpty();
        }
        #endregion

        #region Detail Tests
        [Fact]
        public async Task Detail_WhenIdInvalid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Detail(0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;

            badRequest.Value.Should().Be("Invalid input !");
        }

        [Fact]
        public async Task Detail_WhenProductNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = false
                });

            // Act
            var result = await _controller.Detail(1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Detail_WhenProductExists_ShouldReturnViewResult()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 1000,
                Images = new List<ProductImage>()
            };

            var reviews = new List<Review>
    {
        new Review
        {
            Rating = 5,
            Comment = "Good"
        }
    };

            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = product
                });

            _reviewServiceMock
                .Setup(x => x.GetProductReviewsAsync(1))
                .ReturnsAsync(reviews);

            // Act
            var result = await _controller.Detail(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Detail_ShouldReturnProductDetailViewModel()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 1000,
                Images = new List<ProductImage>()
            };

            var reviews = new List<Review>
    {
        new Review
        {
            Rating = 5,
            Comment = "Excellent"
        }
    };

            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = product
                });

            _reviewServiceMock
                .Setup(x => x.GetProductReviewsAsync(1))
                .ReturnsAsync(reviews);

            // Act
            var result = await _controller.Detail(1);

            // Assert
            var viewResult = result as ViewResult;

            viewResult.Model.Should()
                .BeOfType<ProductDetailViewModel>();
        }

        [Fact]
        public async Task Detail_ShouldMapProductDataCorrectly()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 1500,
                Description = "Gaming Laptop",
                Images = new List<ProductImage>()
            };

            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = product
                });

            _reviewServiceMock
                .Setup(x => x.GetProductReviewsAsync(1))
                .ReturnsAsync(new List<Review>());

            // Act
            var result = await _controller.Detail(1);

            // Assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as ProductDetailViewModel;

            model.Name.Should().Be("Laptop");

            model.Price.Should().Be(1500);

            model.Description.Should().Be("Gaming Laptop");
        }

        [Fact]
        public async Task Detail_ShouldCallGetProductByIdAsync()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = new Product
                    {
                        Images = new List<ProductImage>()
                    }
                });

            _reviewServiceMock
                .Setup(x => x.GetProductReviewsAsync(1))
                .ReturnsAsync(new List<Review>());

            // Act
            await _controller.Detail(1);

            // Assert
            _productServiceMock.Verify(
                x => x.GetProductByIdAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task Detail_ShouldCallGetProductReviewsAsync()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = new Product
                    {
                        Images = new List<ProductImage>()
                    }
                });

            _reviewServiceMock
                .Setup(x => x.GetProductReviewsAsync(1))
                .ReturnsAsync(new List<Review>());

            // Act
            await _controller.Detail(1);

            // Assert
            _reviewServiceMock.Verify(
                x => x.GetProductReviewsAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task Detail_WhenNoReviews_ShouldStillReturnView()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Images = new List<ProductImage>()
            };

            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new ServiceResult<Product>
                {
                    IsSuccess = true,
                    Data = product
                });

            _reviewServiceMock
                .Setup(x => x.GetProductReviewsAsync(1))
                .ReturnsAsync(new List<Review>());

            // Act
            var result = await _controller.Detail(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
        #endregion

        #region ByCategory Tests
        [Fact]
        public async Task ByCategory_SetsViewBagAndReturnsProducts()
        {
            // Arrange
            int catId = 1;
            var category = new Category { Id = catId, Name = "Electronics", Description = "Desc" };
            var products = new List<Product> { new Product { Id = 10, Name = "Phone" } };

            _categoryServiceMock.Setup(s => s.GetByIdAsync(catId))
                .ReturnsAsync(new ServiceResult<Category> { IsSuccess = true, Data = category });

            _productServiceMock.Setup(s => s.GetByCategory(catId))
                .ReturnsAsync(new ServiceResult<List<Product>> { IsSuccess = true, Data = products });

            // Act
            var result = await _controller.ByCategory(catId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Electronics", _controller.ViewBag.CategoryName);
            Assert.Equal("Desc", _controller.ViewBag.CategoryDescription);

            var model = Assert.IsType<List<ProductViewModel>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task ByCategory_ReturnsEmptyList_WhenNoProductsFound()
        {
            // Arrange
            int catId = 1;
            _categoryServiceMock.Setup(s => s.GetByIdAsync(catId))
                .ReturnsAsync(new ServiceResult<Category> { IsSuccess = true });

            _productServiceMock.Setup(s => s.GetByCategory(catId))
                .ReturnsAsync(new ServiceResult<List<Product>> { IsSuccess = true, Data = new List<Product>() });

            // Act
            var result = await _controller.ByCategory(catId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<ProductViewModel>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task ByCategory_ReturnsMessage_WhenServiceFails()
        {
            // Arrange
            int catId = 1;
            _categoryServiceMock.Setup(s => s.GetByIdAsync(catId))
                .ReturnsAsync(new ServiceResult<Category> { IsSuccess = true });

            _productServiceMock.Setup(s => s.GetByCategory(catId))
                .ReturnsAsync(new ServiceResult<List<Product>> { IsSuccess = false, Message = "Failed" });

            // Act
            var result = await _controller.ByCategory(catId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Failed", viewResult.ViewName);
            Assert.Null(viewResult.Model);
        }
        #endregion
    }
}
