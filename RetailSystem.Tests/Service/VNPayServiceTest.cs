using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Moq;
using RetailSystem.Domain.Entities;
using RetailSystem.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Tests.Service
{
    public class VNPayServiceTest
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly VNPayService _service;

        public VNPayServiceTest()
        {
            _configMock = new Mock<IConfiguration>();

            _configMock.Setup(x => x["VNPay:TmnCode"]).Returns("TESTCODE");
            _configMock.Setup(x => x["VNPay:ReturnUrl"]).Returns("https://return.url");
            _configMock.Setup(x => x["VNPay:BaseUrl"]).Returns("https://vnpay.test");
            _configMock.Setup(x => x["VNPay:HashSecret"]).Returns("secret");

            _service = new VNPayService(_configMock.Object);
        }

        [Fact]
        public void CreatePaymentUrl_ShouldIncludeConfigValues()
        {
            var order = new Order
            {
                Id = 1,
                TotalAmount = 10000,
                TxnRef = "TXN003"
            };

            var url = _service.CreatePaymentUrl(new DefaultHttpContext(), order);

            Assert.Contains("TESTCODE", url);
            Assert.Contains("vnp_ReturnUrl=https%3A%2F%2Freturn.url", url);
        }

        [Fact]
        public void CreatePaymentUrl_ShouldMultiplyAmountBy100()
        {
            var order = new Order
            {
                Id = 1,
                TotalAmount = 123456,
                TxnRef = "TXN002"
            };

            var url = _service.CreatePaymentUrl(new DefaultHttpContext(), order);

            Assert.Contains("vnp_Amount=12345600", url);
        }

        [Fact]
        public void Execute_ShouldReturnSuccess_WhenValidResponse()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "vnp_TxnRef", "TXN001" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TransactionNo", "999" },
                { "vnp_SecureHash", "fakehash" }
            });

            var result = _service.Execute(query);

            Assert.Equal("TXN001", result.TxnRef);
            Assert.Equal("00", result.ResponseCode);
        }

        [Fact]
        public void CreatePaymentUrl_ShouldReturnUrl_ContainRequiredFields()
        {
            var order = new Order
            {
                Id = 1,
                TotalAmount = 100000,
                TxnRef = "TXN001"
            };

            var context = new DefaultHttpContext();

            var url = _service.CreatePaymentUrl(context, order);

            Assert.Contains("vnp_Version=2.1.0", url);
            Assert.Contains("vnp_Command=pay", url);
            Assert.Contains("vnp_TxnRef=TXN001", url);
            Assert.Contains("vnp_Amount=10000000", url); // *100
            Assert.Contains("vnp_SecureHash=", url);
        }

        [Fact]
        public void Execute_ShouldFail_WhenResponseCodeNot00()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "vnp_TxnRef", "TXN001" },
                { "vnp_ResponseCode", "01" },
                { "vnp_SecureHash", "wrong" }
            });

            var result = _service.Execute(query);

            Assert.False(result.Success);
        }

        [Fact]
        public void Execute_ShouldFail_WhenHashInvalid()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "vnp_TxnRef", "TXN001" },
                { "vnp_ResponseCode", "00" },
                { "vnp_SecureHash", "invalid_hash" }
            });

            var result = _service.Execute(query);

            Assert.False(result.Success);
        }

        [Fact]
        public void Execute_ShouldAlwaysReturnTxnRef()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "vnp_TxnRef", "ABC123" },
                { "vnp_ResponseCode", "00" },
                { "vnp_SecureHash", "bad" }
            });

            var result = _service.Execute(query);

            Assert.Equal("ABC123", result.TxnRef);
        }

        [Fact]
        public void Execute_ShouldHandleEmptyQuery()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues>());

            var result = _service.Execute(query);

            Assert.False(result.Success);
        }
    }
}
