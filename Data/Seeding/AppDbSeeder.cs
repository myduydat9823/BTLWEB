using BTLWEB.Common;
using BTLWEB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Data;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, IConfiguration? configuration = null)
    {
        await SeedRolesAsync(dbContext);
        await SeedAdminAsync(dbContext, configuration);

        if (!await dbContext.Categories.AnyAsync())
        {
            var categories = GetCategories();
            await dbContext.Categories.AddRangeAsync(categories);
            await dbContext.SaveChangesAsync();
        }

        if (await dbContext.Posts.AnyAsync())
        {
            return;
        }

        var categoryIds = await dbContext.Categories
            .ToDictionaryAsync(x => x.Slug, x => x.Id);

        var posts = BuildPosts(categoryIds);
        await dbContext.Posts.AddRangeAsync(posts);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AppDbContext dbContext)
    {
        var existingRoles = await dbContext.Roles
            .ToListAsync();

        var roles = new[]
        {
            new Role
            {
                RoleName = RoleNames.Member,
                DisplayName = "Thành viên / Hội viên",
                Description = "Người dùng đã đăng ký tài khoản."
            },
            new Role
            {
                RoleName = RoleNames.GiamKhao,
                DisplayName = "Giám khảo",
                Description = "Người dùng có quyền truy cập khu vực giám khảo."
            },
            new Role
            {
                RoleName = RoleNames.Admin,
                DisplayName = "Quản trị viên",
                Description = "Quản trị tài khoản, phân quyền và nội dung hệ thống."
            }
        };

        var missingRoles = roles
            .Where(x => existingRoles.All(role => role.RoleName != x.RoleName))
            .ToList();

        foreach (var role in roles)
        {
            var existingRole = existingRoles.FirstOrDefault(x => x.RoleName == role.RoleName);
            if (existingRole is null)
            {
                continue;
            }

            existingRole.DisplayName = role.DisplayName;
            existingRole.Description = role.Description;
        }

        if (missingRoles.Count == 0)
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        await dbContext.Roles.AddRangeAsync(missingRoles);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(AppDbContext dbContext, IConfiguration? configuration)
    {
        if (configuration is null || await dbContext.Users.AnyAsync(x => x.Role != null && x.Role.RoleName == RoleNames.Admin))
        {
            return;
        }

        var username = configuration["SeedAdmin:Username"];
        var email = configuration["SeedAdmin:Email"];
        var fullName = configuration["SeedAdmin:FullName"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var adminRole = await dbContext.Roles.FirstAsync(x => x.RoleName == RoleNames.Admin);
        var admin = new User
        {
            Username = username.Trim(),
            NormalizedUsername = username.Trim().ToUpperInvariant(),
            Email = email.Trim(),
            NormalizedEmail = email.Trim().ToUpperInvariant(),
            FullName = fullName.Trim(),
            RoleId = adminRole.RoleId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, password);
        await dbContext.Users.AddAsync(admin);
        await dbContext.SaveChangesAsync();
    }

    private static List<Category> GetCategories()
    {
        return
        [
            new Category { Name = "Tin tuc", Slug = "tin-tuc", Description = "Tin moi ve nhiep anh va doi song nghe anh." },
            new Category { Name = "Anh noi bat", Slug = "anh-noi-bat", Description = "Nhung bo anh noi bat duoc chon loc." },
            new Category { Name = "Cuoc thi anh", Slug = "cuoc-thi-anh", Description = "Thong tin cuoc thi, the le va ket qua." },
            new Category { Name = "Trien lam anh Online", Slug = "trien-lam-anh-online", Description = "Khong gian trung bay anh truc tuyen." },
            new Category { Name = "Anh va Doi song", Slug = "anh-va-doi-song", Description = "Goc nhin thi giac ve doi song thuong nhat." },
            new Category { Name = "Du lich - Van hoa - Xa hoi", Slug = "du-lich-van-hoa-xa-hoi", Description = "Cac cau chuyen anh gan voi hanh trinh va van hoa." },
            new Category { Name = "VAPA", Slug = "vapa", Description = "Thong tin hoat dong cua hoi nghe si nhiep anh." },
            new Category { Name = "Media", Slug = "media", Description = "Video, podcast va noi dung da phuong tien." }
        ];
    }

    private static List<Post> BuildPosts(IReadOnlyDictionary<string, int> categoryIds)
    {
        var now = DateTime.UtcNow;

        return
        [
            CreatePost(
                categoryIds["tin-tuc"],
                "tin-tuc-thi-truong-may-anh-2026",
                "Thi truong may anh 2026: nhung chuyen dong dang chu y",
                "Tong hop nhanh cac xu huong moi o phan khuc mirrorless, ong kinh va AI trong nhiep anh.",
                "https://nhiepanhdoisong.vn/quan-khu-9-ky-niem-80-nam-ngay-truyen-thong-bo-tham-muu-va-don-nhan-huan-chuong-bao-ve-to-quoc-hang-nhat-25646.html",
                now.AddDays(-1),
                2840,
                true),
            CreatePost(
                categoryIds["tin-tuc"],
                "tin-tuc-nhiep-anh-di-dong",
                "Nhiep anh di dong dang thay doi cach ke chuyen bang hinh anh",
                "Nhung nang cap ve cam bien, xu ly anh va workflow chia se dang mo rong gioi han sang tao.",
                "https://placehold.co/900x600/1d6996/ffffff?text=Mobile",
                now.AddDays(-3),
                1520,
                false),
            CreatePost(
                categoryIds["anh-noi-bat"],
                "anh-noi-bat-sac-mau-mua-he",
                "Bo anh sac mau mua he trong nhip song pho thi",
                "Nhung khoanh khac doi thuong duoc bat lai bang bang mau manh va bo cuc gon gang.",
                "https://placehold.co/900x600/357266/ffffff?text=Anh+Noi+Bat",
                now.AddDays(-2),
                3180,
                true),
            CreatePost(
                categoryIds["anh-noi-bat"],
                "anh-noi-bat-chan-dung-nghe-nhan",
                "Chan dung nghe nhan va cau chuyen phia sau anh sang",
                "Tac pham tap trung vao tay nghe, chat lieu va khong gian lam viec cua nguoi nghe.",
                "https://placehold.co/900x600/775144/ffffff?text=Portrait",
                now.AddDays(-6),
                940,
                false),
            CreatePost(
                categoryIds["cuoc-thi-anh"],
                "cuoc-thi-anh-ky-niem-thanh-pho",
                "Phat dong cuoc thi anh Ky niem thanh pho 2026",
                "Cuoc thi huong toi nhung goc nhin moi ve do thi, con nguoi va ky uc tap the.",
                "https://placehold.co/900x600/8d5524/ffffff?text=Contest",
                now.AddDays(-4),
                1875,
                true),
            CreatePost(
                categoryIds["cuoc-thi-anh"],
                "cuoc-thi-anh-giai-thuong-thang",
                "10 tac pham vao vong trong giai thuong thang",
                "Danh sach tac pham duoc hoi dong chuyen mon danh gia cao ve y tuong va ky thuat.",
                "https://placehold.co/900x600/5f0f40/ffffff?text=Award",
                now.AddDays(-8),
                1260,
                false),
            CreatePost(
                categoryIds["trien-lam-anh-online"],
                "trien-lam-online-khong-gian-song",
                "Trien lam online Khong gian song va nhung khoanh khac nho",
                "Chu de ton vinh nhung chi tiet nho, gan gui ma de bi bo qua trong doi song hang ngay.",
                "https://placehold.co/900x600/3a506b/ffffff?text=Exhibition",
                now.AddDays(-5),
                1125,
                true),
            CreatePost(
                categoryIds["trien-lam-anh-online"],
                "trien-lam-online-anh-den-trang",
                "Chuoi trung bay anh den trang cua nhiep anh tre",
                "Bo suu tap noi bat nho su tiet che ve anh sang va cam xuc ke chuyen bang hinh.",
                "https://placehold.co/900x600/222222/ffffff?text=B%26W",
                now.AddDays(-11),
                770,
                false),
            CreatePost(
                categoryIds["anh-va-doi-song"],
                "anh-va-doi-song-nhip-song-cho-som",
                "Nhip song cho som duoc ke lai qua anh tai lieu",
                "Cac khuon hinh gan voi lao dong, am thanh va net giao tiep quen thuoc trong cho som.",
                "https://placehold.co/900x600/588157/ffffff?text=Life",
                now.AddDays(-7),
                1390,
                true),
            CreatePost(
                categoryIds["anh-va-doi-song"],
                "anh-va-doi-song-tre-em-vung-cao",
                "Tre em vung cao trong mot ngay hoc dac biet",
                "Bo anh ghi lai niem vui, su tap trung va nhung tuong tac nho trong lop hoc.",
                "https://placehold.co/900x600/bc6c25/ffffff?text=Children",
                now.AddDays(-12),
                880,
                false),
            CreatePost(
                categoryIds["du-lich-van-hoa-xa-hoi"],
                "du-lich-van-hoa-xa-hoi-mien-bien",
                "Mien bien luc rinh sang: hanh trinh tim mau sac ban dia",
                "Bai viet khai thac cach nguoi chup anh tiep can van hoa dia phuong bang su ton trong.",
                "https://placehold.co/900x600/006d77/ffffff?text=Travel",
                now.AddDays(-9),
                1680,
                true),
            CreatePost(
                categoryIds["du-lich-van-hoa-xa-hoi"],
                "du-lich-van-hoa-xa-hoi-le-hoi-lang",
                "Le hoi lang trong ong kinh cua tac gia tre",
                "Khoanh khac dam dac tinh cong dong va su giao thoa giua truyen thong voi hien dai.",
                "https://placehold.co/900x600/9c6644/ffffff?text=Culture",
                now.AddDays(-13),
                910,
                false),
            CreatePost(
                categoryIds["vapa"],
                "vapa-hoat-dong-chuyen-mon-quy-3",
                "VAPA cong bo lich hoat dong chuyen mon quy 3",
                "Lich workshop, toa dam va cac chuong trinh ket noi hoi vien trong quy toi.",
                "https://placehold.co/900x600/1b4965/ffffff?text=VAPA",
                now.AddDays(-10),
                1435,
                true),
            CreatePost(
                categoryIds["vapa"],
                "vapa-bao-tro-nghe-si-tre",
                "Chuong trinh ho tro nghe si tre mo don dang ky",
                "Sang kien moi nham ho tro tac gia tre phat trien du an ca nhan va trien lam nho.",
                "https://placehold.co/900x600/5c677d/ffffff?text=Support",
                now.AddDays(-15),
                640,
                false),
            CreatePost(
                categoryIds["media"],
                "media-podcast-hau-truong",
                "Podcast hau truong: khi buc anh duoc tao ra nhu the nao",
                "Tap podcast moi chia se quy trinh len y tuong, di chuyen va xu ly hau ky cho bo anh pho.",
                "https://placehold.co/900x600/7b2cbf/ffffff?text=Podcast",
                now.AddDays(-14),
                1990,
                true),
            CreatePost(
                categoryIds["media"],
                "media-video-phong-van",
                "Video phong van nhiep anh gia ve cau chuyen anh tai lieu",
                "Cuoc tro chuyen tap trung vao dao duc hinh anh, su kien nhan vat va boi canh tac nghiep.",
                "https://placehold.co/900x600/9d4edd/ffffff?text=Video",
                now.AddDays(-18),
                810,
                false)
        ];
    }

    private static Post CreatePost(
        int categoryId,
        string slug,
        string title,
        string summary,
        string thumbnailUrl,
        DateTime publishedAt,
        int viewCount,
        bool isFeatured)
    {
        return new Post
        {
            CategoryId = categoryId,
            Slug = slug,
            Title = title,
            Summary = summary,
            ThumbnailUrl = thumbnailUrl,
            Content = $"{title}\n\n{summary}",
            PublishedAt = publishedAt,
            ViewCount = viewCount,
            IsFeatured = isFeatured,
            Status = PostStatus.Published
        };
    }
}
