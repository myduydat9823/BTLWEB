using BTLWEB.Models;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Slug)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Slug)
                .HasMaxLength(280)
                .IsRequired();

            entity.Property(x => x.Summary)
                .HasMaxLength(600);

            entity.Property(x => x.ThumbnailUrl)
                .HasMaxLength(500);

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Content)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.PublishedAt)
                .HasColumnType("datetime2");

            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.PublishedAt);
            entity.HasIndex(x => x.ViewCount);
            entity.HasIndex(x => new { x.IsFeatured, x.PublishedAt });

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Posts)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
