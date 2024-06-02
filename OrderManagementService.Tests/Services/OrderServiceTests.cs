using NUnit.Framework;
using Moq;
using OrderManagementService.Services;
using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using FrontendService.Hubs;
using System.Threading.Tasks;

namespace OrderManagementService.Tests.Services
{
    [TestFixture]
    public class OrderServiceTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILogger<OrderService>> _loggerMock;
        private Mock<IHubContext<UpdateHub>> _hubContextMock;
        private OrderService _orderService;

        [SetUp]
        public void SetUp()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _hubContextMock = new Mock<IHubContext<UpdateHub>>();

            _orderService = new OrderService(
                _httpClientFactoryMock.Object,
                _configurationMock.Object,
                _loggerMock.Object,
                _hubContextMock.Object);
        }

        [Test]
        public async Task GetPathAsync_ValidPathName_ReturnsPath()
        {
            // Arrange
            var pathName = "testPath";
            var expectedPath = "test/path";
            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent($"{{ \"{pathName}\": \"{expectedPath}\" }}")
                });
            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

            // Act
            var result = await _orderService.GetPathAsync(pathName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedPath));
        }

        // Additional test methods...
    }
}
