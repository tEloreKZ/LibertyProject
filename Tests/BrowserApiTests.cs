using ProjectLiberty.UI;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class BrowserApiTests
{
    [Theory]
    [InlineData("file:///C:/secret.html")]
    [InlineData("https://example.com")]
    [InlineData("liberty://unknown/index.html")]
    public void CreateRejectsUrlsOutsideWhitelistedSchemes(string url)
    {
        Assert.Throws<ArgumentException>(() => Browser.Create(url));
    }

    [Fact]
    public void CreateValidatesOptionsBeforeCallingNativeCore()
    {
        var options = new BrowserOptions { Width = 0 };
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Browser.Create("liberty://ui/test/index.html", options));
    }

    [Fact]
    public void DispatchRejectsInvalidBatchSizeWithoutNativeCore()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Browser.DispatchPendingEvents(0));
    }
}
