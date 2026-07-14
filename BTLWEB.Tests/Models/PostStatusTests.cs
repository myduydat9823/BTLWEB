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
}
