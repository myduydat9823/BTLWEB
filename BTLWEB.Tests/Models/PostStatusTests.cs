using BTLWEB.Models;

namespace BTLWEB.Tests.Models;

public class PostStatusTests
{
    [Fact]
    public void All_ShouldContainStatusesRequiredByArticleManagement()
    {
        var expectedStatuses = new[]
        {
            PostStatus.Draft,
            PostStatus.Pending,
            PostStatus.Approved,
            PostStatus.Rejected
        };

        Assert.Equal(expectedStatuses, PostStatus.All);
    }

    [Theory]
    [InlineData(PostStatus.Approved)]
    [InlineData(PostStatus.LegacyPublished)]
    public void IsVisibleStatus_ShouldAcceptApprovedAndLegacyPublishedPosts(string status)
    {
        Assert.True(PostStatus.IsVisibleStatus(status));
    }

    [Theory]
    [InlineData(PostStatus.LegacyPublished, PostStatus.Approved)]
    [InlineData(PostStatus.LegacyHidden, PostStatus.Rejected)]
    [InlineData(PostStatus.LegacyArchived, PostStatus.Rejected)]
    public void Normalize_ShouldMapLegacyStatusesToReviewStatuses(string legacyStatus, string expectedStatus)
    {
        Assert.Equal(expectedStatus, PostStatus.Normalize(legacyStatus));
    }
}
