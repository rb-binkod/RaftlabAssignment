using RaftlabAssignment.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RaftlabAssignment.Infrastructure.Models
{
    public class ApiUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("first_name")]
        public string First_Name { get; set; }

        [JsonPropertyName("last_name")]
        public string Last_Name { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }
    }

    public class ApiUserListResponse
    {

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("total_pages")]
        public int Total_Pages { get; set; }

        [JsonPropertyName("data")]
        public List<ApiUser> Data { get; set; }
    }
}
