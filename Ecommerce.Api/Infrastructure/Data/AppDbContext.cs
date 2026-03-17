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
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<ProductView> ProductViews => Set<ProductView>();
    public DbSet<SiteVisit> SiteVisits => Set<SiteVisit>();

    // ✅ Appearance / Ads
    public DbSet<AppearanceConfig> AppearanceConfigs => Set<AppearanceConfig>();
    public DbSet<AppearanceAd> AppearanceAds => Set<AppearanceAd>();

    // ✅ Ads (منفصلة عن Appearance)
    public DbSet<Ad> Ads => Set<Ad>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

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
            .Property(p => p.Category)
            .HasMaxLength(80)
            .HasDefaultValue("general");

        modelBuilder.Entity<Product>()
            .Property(p => p.SubCategory)
            .HasMaxLength(120)
            .HasDefaultValue("");

        modelBuilder.Entity<Product>()
            .Property(p => p.StockQuantity)
            .HasDefaultValue(100);

        modelBuilder.Entity<Product>()
            .Property(p => p.LowStockThreshold)
            .HasDefaultValue(5);

        modelBuilder.Entity<Product>()
            .Property(p => p.IsCouponAllowed)
            .HasDefaultValue(true);

        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.Category, p.SubCategory, p.IsPublished });

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

        modelBuilder.Entity<SiteVisit>()
            .HasIndex(v => v.CreatedAt);

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

        // =========================
        // ✅ Appearance / Ads
        // =========================
        modelBuilder.Entity<AppearanceConfig>()
            .Property(x => x.EnabledThemesJson)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AppearanceConfig>()
            .Property(x => x.EnabledEffectsJson)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AppearanceConfig>()
            .HasMany(x => x.Ads)
            .WithOne(x => x.AppearanceConfig)
            .HasForeignKey(x => x.AppearanceConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppearanceAd>()
            .HasIndex(x => new { x.AppearanceConfigId, x.SortOrder });

        modelBuilder.Entity<AppearanceAd>()
            .Property(x => x.ImageUrl)
            .HasMaxLength(2000);

        modelBuilder.Entity<AppearanceAd>()
            .Property(x => x.LinkUrl)
            .HasMaxLength(1000);

        // =========================
        // ✅ Ads (منفصلة)
        // =========================
        modelBuilder.Entity<Ad>()
            .Property(x => x.Placement)
            .HasMaxLength(120);

        modelBuilder.Entity<Ad>()
            .Property(x => x.ImageUrl)
            .HasMaxLength(2000);

        modelBuilder.Entity<Ad>()
            .Property(x => x.LinkUrl)
            .HasMaxLength(1000);

        modelBuilder.Entity<Ad>()
            .Property(x => x.ImageUrlsJson)
            .HasColumnType("jsonb");

        modelBuilder.Entity<Ad>()
            .HasIndex(x => new { x.Type, x.Placement, x.SortOrder });

        modelBuilder.Entity<ProductReview>()
            .Property(x => x.Comment)
            .HasMaxLength(1500);

        modelBuilder.Entity<ProductReview>()
            .HasIndex(x => new { x.ProductId, x.UserId })
            .IsUnique();

        modelBuilder.Entity<ProductReview>()
            .HasIndex(x => new { x.ProductId, x.CreatedAt });

        modelBuilder.Entity<ProductReview>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductReview>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Coupon>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<Coupon>()
            .Property(x => x.Code)
            .HasMaxLength(80);

        modelBuilder.Entity<Coupon>()
            .Property(x => x.Title)
            .HasMaxLength(160);

        modelBuilder.Entity<Order>()
            .Property(x => x.CouponCode)
            .HasMaxLength(80);

        modelBuilder.Entity<CouponUsage>()
            .Property(x => x.DeviceKeyHash)
            .HasMaxLength(128);

        modelBuilder.Entity<CouponUsage>()
            .HasIndex(x => new { x.CouponId, x.DeviceKeyHash })
            .IsUnique();

        modelBuilder.Entity<CouponUsage>()
            .HasIndex(x => new { x.CouponId, x.UserId });

    }
}
