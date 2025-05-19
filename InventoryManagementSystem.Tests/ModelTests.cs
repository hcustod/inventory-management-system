using InventoryManagementSystem.Models;
using Xunit;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace InventoryManagementSystem.Tests
{
    public class ModelTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Product_ShouldBeInvalid_IfNameIsEmpty()
        {
            var product = new Product
            {
                Name = "", // invalid
                Price = 10.0m,
                ProductStockAmount = 5
            };

            var errors = ValidateModel(product);
            Assert.Contains(errors, e => e.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Product_ShouldBeInvalid_IfPriceIsNegative()
        {
            var product = new Product
            {
                Name = "Test Product",
                Price = -5.00m, // invalid
                ProductStockAmount = 3
            };

            var errors = ValidateModel(product);
            Assert.Contains(errors, e => e.MemberNames.Contains("Price"));
        }

        [Fact]
        public void Category_ShouldBeInvalid_IfNameIsEmpty()
        {
            var category = new Category
            {
                CategoryName = "" // invalid
            };

            var errors = ValidateModel(category);
            Assert.Contains(errors, e => e.MemberNames.Contains("CategoryName"));
        }

        [Fact]
        public void Order_ShouldCalculateTotalCorrectly()
        {
            var order = new Order
            {
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct { Quantity = 2, Product = new Product { Price = 15.5m } },
                    new OrderProduct { Quantity = 1, Product = new Product { Price = 4.5m } }
                }
            };

            var expectedTotal = 2 * 15.5m + 1 * 4.5m;
            var actualTotal = order.OrderProducts.Sum(op => op.Product.Price * op.Quantity);

            Assert.Equal(expectedTotal, actualTotal);
        }

        [Fact]
        public void User_ShouldBeInvalid_IfFullNameOrContactInfoMissing()
        {
            var user = new User
            {
                FullName = null,
                ContactInformation = null
            };

            var errors = ValidateModel(user);
            Assert.Contains(errors, e => e.MemberNames.Contains("FullName"));
            Assert.Contains(errors, e => e.MemberNames.Contains("ContactInformation"));
        }
    }
}
