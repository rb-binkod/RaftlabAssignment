using Microsoft.Extensions.DependencyInjection;
using RaftlabAssignment.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RaftlabAssessment.WebApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = Startup.ConfigureServices();
            var userService = serviceProvider.GetRequiredService<IExternalUserService>();

            try
            {
                Console.WriteLine("Fetching single user with ID 2...");
                var user = await userService.GetUserByIdAsync(2);
                Console.WriteLine($"User: {user.FirstName} {user.LastName}, Email: {user.Email}");

                Console.WriteLine("\nFetching all users...");
                var allUsers = await userService.GetAllUsersAsync();
                foreach (var u in allUsers)
                {
                    Console.WriteLine($"ID: {u.Id}, Name: {u.FirstName} {u.LastName}, Email: {u.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
