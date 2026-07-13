namespace BTLWEB.Models;

public static class PostStatus
{
    public const string Draft = "Draft";
    public const string Published = "Published";
    public const string Hidden = "Hidden";
    public const string Archived = "Archived";

    public static readonly string[] All = [Draft, Published, Hidden, Archived];
}
