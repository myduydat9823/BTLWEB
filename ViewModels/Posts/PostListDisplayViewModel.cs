namespace BTLWEB.ViewModels;

public sealed record PostListDisplayViewModel(
    List<PostCardViewModel> Posts,
    string FallbackImage,
    bool ShowRank);
