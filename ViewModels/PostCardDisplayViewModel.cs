namespace BTLWEB.ViewModels;

public sealed record PostCardDisplayViewModel(
    PostCardViewModel Post,
    string FallbackImage,
    string CssClass);
