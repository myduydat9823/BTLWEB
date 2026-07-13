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
    public DbSet<BTLWEB.Models.Competition.Competition> Competitions => Set<BTLWEB.Models.Competition.Competition>();
    public DbSet<BTLWEB.Models.Competition.CompetitionEntry> CompetitionEntries => Set<BTLWEB.Models.Competition.CompetitionEntry>();
    public DbSet<BTLWEB.Models.Competition.Photo> Photos => Set<BTLWEB.Models.Competition.Photo>();

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

        modelBuilder.Entity<BTLWEB.Models.Competition.Competition>(entity =>
        {
            entity.ToTable("Competitions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.Rules)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.SubmissionStartDate)
                .HasColumnType("datetime2");

            entity.Property(x => x.SubmissionEndDate)
                .HasColumnType("datetime2");

            entity.Property(x => x.CreatedAt)
                .HasColumnType("datetime2");

            entity.Property(x => x.UpdatedAt)
                .HasColumnType("datetime2");

            entity.HasIndex(x => x.Status);

            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BTLWEB.Models.Competition.Photo>(entity =>
        {
            entity.ToTable("Photos");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.ImagePath)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.FileExtension)
                .HasMaxLength(20);

            entity.Property(x => x.UploadedAt)
                .HasColumnType("datetime2");

            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BTLWEB.Models.Competition.CompetitionEntry>(entity =>
        {
            entity.ToTable("CompetitionEntries");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SubmittedAt)
                .HasColumnType("datetime2");

            entity.Property(x => x.AdminNote)
                .HasMaxLength(500);

            entity.HasIndex(x => new { x.CompetitionId, x.UserId }).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.Rank);

            entity.HasOne(x => x.Competition)
                .WithMany(x => x.Entries)
                .HasForeignKey(x => x.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Photo)
                .WithMany()
                .HasForeignKey(x => x.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
