namespace BTLWEB.Models;

public static class PostStatus
{
    public const string Draft = "Draft";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string LegacyPublished = "Published";
    public const string LegacyHidden = "Hidden";
    public const string LegacyArchived = "Archived";

    public const string Published = Approved;
    public const string Hidden = Rejected;
    public const string Archived = Rejected;

    public static readonly string[] All = [Draft, Pending, Approved, Rejected];
    public static readonly string[] VisibleStatuses = [Approved, LegacyPublished];
    public static readonly string[] RejectedStatuses = [Rejected, LegacyHidden, LegacyArchived];

    public static string Normalize(string? status)
    {
        return status switch
        {
            LegacyPublished => Approved,
            LegacyHidden or LegacyArchived => Rejected,
            Pending or Approved or Rejected or Draft => status,
            _ => Draft
        };
    }

    public static bool IsKnownStatus(string? status)
    {
        return All.Contains(status) || status is LegacyPublished or LegacyHidden or LegacyArchived;
    }

    public static bool IsReviewStatus(string? status)
    {
        return Normalize(status) is Pending or Approved or Rejected;
    }

    public static bool IsVisibleStatus(string? status)
    {
        return VisibleStatuses.Contains(status);
    }

    public static string GetDisplayText(string? status)
    {
        return Normalize(status) switch
        {
            Pending => "Chờ duyệt",
            Approved => "Đã duyệt",
            Rejected => "Từ chối",
            _ => "Nháp"
        };
    }

    public static string GetCssClass(string? status)
    {
        return Normalize(status) switch
        {
            Pending => "status-pill--warning",
            Approved => string.Empty,
            Rejected => "status-pill--danger",
            _ => "status-pill--muted"
        };
    }
}
