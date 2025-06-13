using Microsoft.Extensions.DependencyInjection;
using RaftlabAssignment.Infrastructure.Interfaces;
using System;
using System.Threading.Tasks;

namespace RaftlabApiAssignment.Api
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Ravi!");
            var serviceProvider = Startup.ConfigureServices();
            var userService = serviceProvider.GetRequiredService<IExternalUserService>();

            try
            {
                Console.WriteLine("Fetching single user with ID 2...");
                var user = await userService.GetUserByIdAsync(2);
                Console.WriteLine($">>> User: {user.FirstName} {user.LastName}, Email: {user.Email}");

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
