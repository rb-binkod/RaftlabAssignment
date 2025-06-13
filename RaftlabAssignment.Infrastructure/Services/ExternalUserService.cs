using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaftlabAssignment.Domain.Entities;
using RaftlabAssignment.Infrastructure.Configuration;
using RaftlabAssignment.Infrastructure.Interfaces;
using RaftlabAssignment.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RaftlabAssignment.Infrastructure.Services
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalUserService> _logger;
        private readonly IMemoryCache _cache;
        //private readonly string _baseUrl;

        public ExternalUserService(HttpClient httpClient, ApiSettings apiSettings,
                                   IOptions<ApiSettings> options,
                                   IMemoryCache cache,
                                   ILogger<ExternalUserService> logger)
        {
            _httpClient = httpClient;
            //_baseUrl = options.Value.BaseUrl;
            _cache = cache;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(apiSettings.BaseUrl);
            // Add the API key header
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiSettings.ApiKey);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var cacheKey = $"User_{userId}";
            if (_cache.TryGetValue(cacheKey, out User user))
                return user;

            Console.WriteLine($"Endpoint: {_httpClient.BaseAddress}users/{userId}");

            var response = await _httpClient.GetAsync($"users/{userId}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"User fetch failed: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var userData = JsonSerializer.Deserialize<ApiUser>(doc.RootElement.GetProperty("data").GetRawText());

            user = new User
            {
                Id = userData.Id,
                Email = userData.Email,
                FirstName = userData.First_Name,
                LastName = userData.Last_Name,
                Avatar = userData.Avatar
            };

            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var cacheKey = "AllUsers";
            if (_cache.TryGetValue(cacheKey, out List<User> users))
                return users;

            users = new List<User>();
            int page = 1;
            bool hasMore;

            do
            {
                Console.WriteLine($"Endpoint: {_httpClient.BaseAddress}users?page={page}");

                var response = await _httpClient.GetAsync($"users?page={page}");
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Page fetch failed: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiUserListResponse>(content);

                users.AddRange(result.Data.Select(u => new User
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.First_Name,
                    LastName = u.Last_Name,
                    Avatar = u.Avatar
                }));

                hasMore = page < result.Total_Pages;
                page++;

            } while (hasMore);

            _cache.Set(cacheKey, users, TimeSpan.FromMinutes(10));
            return users;
        }
    }
}
