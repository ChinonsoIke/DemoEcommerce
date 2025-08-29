using System.Text.Json;
using DemoEcommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoEcommerce.Data
{
    public static class SeedData
    {
        public static async Task SeedFromJsonAsync(AppDbContext context, string jsonPath)
        {
            await context.Database.MigrateAsync();

            if (await context.Categories.AnyAsync())
                return;

            var json = await File.ReadAllTextAsync(jsonPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var categories = JsonSerializer.Deserialize<List<Category>>(json, options)
                              ?? new List<Category>();

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }

}
