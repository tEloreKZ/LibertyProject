using ProjectLiberty.Localization;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class LocalizationServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "ProjectLiberty.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void GetString_FallsBackToEnglish()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "en"));
        Directory.CreateDirectory(Path.Combine(_directory, "ru"));
        File.WriteAllText(Path.Combine(_directory, "en", "core.json"), "{\"onlyEnglish\":\"fallback\"}");
        File.WriteAllText(Path.Combine(_directory, "ru", "core.json"), "{}");
        using var service = new LocalizationService(new TestLogger(), _directory);
        service.LoadTranslations();
        service.SetLanguage("ru");

        Assert.Equal("fallback", service.GetString("core.onlyEnglish"));
    }

    [Fact]
    public void GetStrings_ReturnsLocalizedUiSnapshotWithEnglishFallback()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "en"));
        Directory.CreateDirectory(Path.Combine(_directory, "ru"));
        File.WriteAllText(Path.Combine(_directory, "en", "ui.json"), "{\"title\":\"Home\",\"fallback\":\"Fallback\"}");
        File.WriteAllText(Path.Combine(_directory, "ru", "ui.json"), "{\"title\":\"Главная\"}");
        using var service = new LocalizationService(new TestLogger(), _directory);
        service.LoadTranslations();
        service.SetLanguage("ru");

        var strings = service.GetStrings("ui.");

        Assert.Equal("Главная", strings["ui.title"]);
        Assert.Equal("Fallback", strings["ui.fallback"]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }
}
