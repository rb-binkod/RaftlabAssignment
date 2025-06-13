using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using RaftlabAssignment.Infrastructure.Configuration;
using RaftlabAssignment.Infrastructure.Services;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RaftlabAssignment.Tests
{
    public class ExternalUserServiceTests
    {
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenUserNotInCache()
        {
            // Arrange
            int userId = 1;
            string cacheKey = $"User_{userId}";
            var apiResponse = @"{
                ""data"": {
                    ""id"": 1,
                    ""email"": ""ravi.bhushan@reqres.in"",
                    ""first_name"": ""Ravi"",
                    ""last_name"": ""Bhushan"",
                    ""avatar"": ""https://reqres.in/img/faces/1-image.jpg""
                }
            }";

            // Mock HttpMessageHandler to return dummy API response
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().EndsWith("/users/1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(apiResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://reqres.in/api/")
            };

            // Mock IMemoryCache to simulate cache miss
            var cacheMock = new Mock<IMemoryCache>();
            object cacheEntry = null;
            cacheMock
                .Setup(mc => mc.TryGetValue(cacheKey, out cacheEntry))
                .Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            cacheMock.Setup(mc => mc.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            // Mock ApiSettings and IOptions<ApiSettings>
            var apiSettings = new ApiSettings
            {
                ApiKey = "reqres-free-v1",
                BaseUrl = "https://reqres.in/api/"
            };

            var optionsMock = new Mock<IOptions<ApiSettings>>();
            optionsMock.Setup(o => o.Value).Returns(apiSettings);

            // Mock ILogger
            var loggerMock = new Mock<ILogger<ExternalUserService>>();

            // Create the service
            var service = new ExternalUserService(httpClient, apiSettings, optionsMock.Object, cacheMock.Object, loggerMock.Object);

            // Act
            var result = await service.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Ravi", result.FirstName);
            Assert.Equal("Bhushan", result.LastName);
            Assert.Equal("ravi.bhushan@reqres.in", result.Email);

            // Verify caching
            cacheMock.Verify(mc => mc.CreateEntry(cacheKey), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsersAcrossPages()
        {
            // Arrange
            var page1Json = @"{
                ""page"": 1,
                ""per_page"": 2,
                ""total"": 4,
                ""total_pages"": 2,
                ""data"": [
                    { ""id"": 1, ""email"": ""george@reqres.in"", ""first_name"": ""George"", ""last_name"": ""Bluth"", ""avatar"": ""img1.jpg"" },
                    { ""id"": 2, ""email"": ""janet@reqres.in"", ""first_name"": ""Janet"", ""last_name"": ""Weaver"", ""avatar"": ""img2.jpg"" }
                ]
            }";

                    var page2Json = @"{
                ""page"": 2,
                ""per_page"": 2,
                ""total"": 4,
                ""total_pages"": 2,
                ""data"": [
                    { ""id"": 3, ""email"": ""emma@reqres.in"", ""first_name"": ""Emma"", ""last_name"": ""Wong"", ""avatar"": ""img3.jpg"" },
                    { ""id"": 4, ""email"": ""eve@reqres.in"", ""first_name"": ""Eve"", ""last_name"": ""Holt"", ""avatar"": ""img4.jpg"" }
                ]
            }";

            // Setup HttpClient mock
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(page1Json)
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(page2Json)
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://reqres.in/api/")
            };

            var optionsMock = new Mock<IOptions<ApiSettings>>();
            var apiSettings = new ApiSettings
            {
                BaseUrl = "https://reqres.in/api/"
            };

            optionsMock.Setup(o => o.Value).Returns(apiSettings);

            //var cacheMock = new Mock<IMemoryCache>(); // Optional — not used directly here

            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var loggerMock = new Mock<ILogger<ExternalUserService>>();

            var service = new ExternalUserService(httpClient, apiSettings, optionsMock.Object, memoryCache, loggerMock.Object);

            // Act
            var users = await service.GetAllUsersAsync();

            // Assert
            Assert.NotNull(users);
            var userList = users.ToList();
            Assert.Equal(4, userList.Count);
            Assert.Equal("George", userList[0].FirstName);
            Assert.Equal("Eve", userList[3].FirstName);

            // Optional: Verify SendAsync was called twice (once per page)
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

    }
}
