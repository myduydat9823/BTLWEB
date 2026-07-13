namespace BTLWEB.Models;

public static class PostStatus
{
    public const string Draft = "Draft";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public const string Published = Approved;
    public const string Hidden = Rejected;
    public const string Archived = Rejected;

    public static readonly string[] All = [Draft, Pending, Approved, Rejected];

    public static bool IsReviewStatus(string? status)
    {
        return status is Pending or Approved or Rejected;
    }

    public static bool IsVisibleStatus(string? status)
    {
        return status is Approved or Published;
    }
}
