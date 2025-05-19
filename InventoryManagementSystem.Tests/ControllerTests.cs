using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Controllers;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace InventoryManagementSystem.Tests
{
    public class ControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB_" + System.Guid.NewGuid())
                .Options;

            var context = new AppDbContext(options);

            // Seed Categories
            context.Categories.Add(new Category
            {
                Id = 1,
                CategoryName = "Mock Category",
                CategoryDescription = "Mock Desc"
            });

            // Seed Products
            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Test Product",
                Description = "Mocked product",
                Price = 10,
                ProductStockAmount = 5,
                LowStockThreshold = 2,
                CategoryId = 1
            });

            // Seed Orders
            context.Orders.Add(new Order
            {
                Id = 1,
                userName = "Test User",
                userEmail = "test@example.com",
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct
                    {
                        ProductId = 1,
                        Quantity = 1
                    }
                }
            });

            context.SaveChanges();
            return context;
        }

        private static ControllerContext GetMockedUserContext()
        {
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "TestUser")
                    }, "mock"))
                }
            };
        }

        [Fact]
        public async Task GetProducts_ReturnsOkResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new ProductsApiController(context, new LoggerFactory().CreateLogger<ProductsApiController>())
            {
                ControllerContext = GetMockedUserContext()
            };

            var result = await controller.GetProducts(null, null, null, null, null, null);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetOrders_ReturnsOkResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new OrdersApiController(context, new LoggerFactory().CreateLogger<OrdersApiController>())
            {
                ControllerContext = GetMockedUserContext()
            };

            var result = await controller.GetOrders();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCategories_ReturnsOkResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new CategoriesApiController(context, new LoggerFactory().CreateLogger<CategoriesApiController>())
            {
                ControllerContext = GetMockedUserContext()
            };

            var result = await controller.GetAllCategories();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
