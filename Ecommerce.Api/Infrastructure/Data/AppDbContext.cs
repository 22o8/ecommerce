// Ecommerce.Api/Infrastructure/Data/AppDbContext.cs
using Ecommerce.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();

    // ✅ مهم جداً (كان ناقص ويسبب AppDbContext doesn't contain Brands)
    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<ServiceRequirementTemplate> ServiceRequirements => Set<ServiceRequirementTemplate>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ServiceRequestAnswer> ServiceRequestAnswers => Set<ServiceRequestAnswer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProductAsset> ProductAssets => Set<ProductAsset>();
    public DbSet<DownloadToken> DownloadTokens => Set<DownloadToken>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(200);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasMaxLength(50);

        // Product - Images relation
        modelBuilder.Entity<Product>()
            .HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductImage>()
            .Property(x => x.Url)
            .HasMaxLength(2000);

        modelBuilder.Entity<Product>()
            .Property(p => p.Brand)
            .HasMaxLength(120)
            .HasDefaultValue("Unspecified");

        modelBuilder.Entity<Product>()
            .Property(x => x.RatingAvg)
            .HasPrecision(3, 2); // مثل 4.50

        // =========================
        // ✅ Favorites & Views
        // =========================
        modelBuilder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.ProductId })
            .IsUnique();

        modelBuilder.Entity<ProductView>()
            .HasIndex(v => new { v.ProductId, v.CreatedAt });

        // =========================
        // ✅ Brand config
        // =========================
        modelBuilder.Entity<Brand>()
            .HasIndex(b => b.Slug)
            .IsUnique();

        modelBuilder.Entity<Brand>()
            .Property(b => b.Slug)
            .HasMaxLength(80);

        modelBuilder.Entity<Brand>()
            .Property(b => b.Name)
            .HasMaxLength(120);

        modelBuilder.Entity<Brand>()
            .Property(b => b.Description)
            .HasMaxLength(400);

        modelBuilder.Entity<Brand>()
            .Property(b => b.LogoUrl)
            .HasMaxLength(400);
    }
}
