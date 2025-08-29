using DemoEcommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace DemoEcommerce.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Review> Reviews => Set<Review>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Review>()
                .ToTable(t => t.HasCheckConstraint("CK_Review_Rating_Range", "[Rating] BETWEEN 1 AND 5"));

            // Converter: ReadOnlyMemory<float> <-> string
            var embeddingConverter = new ValueConverter<ReadOnlyMemory<float>, string>(
                v => JsonConvert.SerializeObject(v.ToArray()), // store as JSON string
                v => new ReadOnlyMemory<float>(JsonConvert.DeserializeObject<float[]>(v)!)
            );

            modelBuilder.Entity<Product>()
                .Property(p => p.Embedding)
                .HasConversion(embeddingConverter)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Category>()
                .Property(p => p.Embedding)
                .HasConversion(embeddingConverter)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Review>()
                .Property(p => p.Embedding)
                .HasConversion(embeddingConverter)
                .HasColumnType("nvarchar(max)");
        }
    }

}
