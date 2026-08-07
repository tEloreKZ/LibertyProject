using ProjectLiberty.UI;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class DashboardMenuTests
{
    [Fact]
    public void ShowAndHide_ToggleVisibility()
    {
        var menu = new DashboardMenu("liberty_main", "unused-until-draw");

        menu.Show();
        Assert.True(menu.IsVisible);
        menu.Hide();

        Assert.False(menu.IsVisible);
        Assert.Equal("liberty_main", menu.Id);
    }
}
