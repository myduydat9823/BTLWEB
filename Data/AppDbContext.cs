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
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<UserRoleHistory> UserRoleHistories => Set<UserRoleHistory>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<ArticleAdminLog> ArticleAdminLogs => Set<ArticleAdminLog>();

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

            entity.Property(x => x.MetaTitle)
                .HasMaxLength(250);

            entity.Property(x => x.MetaDescription)
                .HasMaxLength(500);

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Content)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.PublishedAt)
                .HasColumnType("datetime2");

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(x => x.UpdatedAtUtc)
                .HasColumnType("datetime2");

            entity.Property(x => x.DeletedAtUtc)
                .HasColumnType("datetime2");

            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.PublishedAt);
            entity.HasIndex(x => x.ViewCount);
            entity.HasIndex(x => new { x.IsFeatured, x.PublishedAt });
            entity.HasIndex(x => new { x.Status, x.IsDeleted, x.PublishedAt });
            entity.HasIndex(x => x.AuthorId);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Posts)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Author)
                .WithMany(x => x.AuthoredPosts)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DeletedByUser)
                .WithMany(x => x.DeletedPosts)
                .HasForeignKey(x => x.DeletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ArticleAdminLog>(entity =>
        {
            entity.ToTable("ArticleAdminLogs");
            entity.HasKey(x => x.ArticleAdminLogId);

            entity.Property(x => x.Action).HasMaxLength(80).IsRequired();
            entity.Property(x => x.StatusBefore).HasMaxLength(50);
            entity.Property(x => x.StatusAfter).HasMaxLength(50);
            entity.Property(x => x.TitleSnapshot).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(x => new { x.PostId, x.CreatedAtUtc });
            entity.HasIndex(x => new { x.ActorUserId, x.CreatedAtUtc });

            entity.HasOne(x => x.Post)
                .WithMany(x => x.AdminLogs)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ActorUser)
                .WithMany(x => x.ArticleAdminLogs)
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.RoleId);

            entity.Property(x => x.RoleName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.HasIndex(x => x.RoleName).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.UserId);

            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.NormalizedUsername).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.AvatarUrl).HasMaxLength(500);
            entity.Property(x => x.Bio).HasMaxLength(1000);
            entity.Property(x => x.PhoneEncrypted).HasMaxLength(1000);
            entity.Property(x => x.AddressEncrypted).HasMaxLength(2000);
            entity.Property(x => x.DateOfBirthEncrypted).HasMaxLength(1000);

            entity.HasIndex(x => x.NormalizedUsername).IsUnique();
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
            entity.HasIndex(x => new { x.RoleId, x.IsActive, x.IsDeleted });

            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoginLog>(entity =>
        {
            entity.ToTable("LoginLogs");
            entity.HasKey(x => x.LoginLogId);

            entity.Property(x => x.Identifier).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(300);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.HasIndex(x => x.CreatedAtUtc);
            entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc });

            entity.HasOne(x => x.User)
                .WithMany(x => x.LoginLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserRoleHistory>(entity =>
        {
            entity.ToTable("UserRoleHistories");
            entity.HasKey(x => x.UserRoleHistoryId);

            entity.Property(x => x.Note).HasMaxLength(500);
            entity.HasIndex(x => new { x.UserId, x.ChangedAtUtc });

            entity.HasOne(x => x.User)
                .WithMany(x => x.RoleHistories)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OldRole)
                .WithMany()
                .HasForeignKey(x => x.OldRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NewRole)
                .WithMany()
                .HasForeignKey(x => x.NewRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens");
            entity.HasKey(x => x.PasswordResetTokenId);

            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreatedIpAddress).HasMaxLength(64);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });

            entity.HasOne(x => x.User)
                .WithMany(x => x.PasswordResetTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
