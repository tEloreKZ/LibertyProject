using ProjectLiberty.Configuration;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class ConfigurationServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "ProjectLiberty.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void LoadConfiguration_FlattensAndConvertsNestedValues()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "core.json"), "{\"game\":{\"tickRate\":60},\"enabled\":true}");
        using var service = new ConfigurationService(new TestLogger(), _directory);

        service.LoadConfiguration();

        Assert.Equal(60, service.GetValue<int>("game.tickRate"));
        Assert.True(service.GetValue<bool>("enabled"));
    }

    [Fact]
    public void Save_PreservesNestedSections()
    {
        Directory.CreateDirectory(_directory);
        using var service = new ConfigurationService(new TestLogger(), _directory);
        service.SetValue("plugins.example.enabled", true);

        service.Save();
        using var reloaded = new ConfigurationService(new TestLogger(), _directory);
        reloaded.LoadConfiguration();

        Assert.True(reloaded.GetValue<bool>("plugins.example.enabled"));
    }

    [Fact]
    public void Reload_KeepsPreviousValuesWhenJsonIsInvalid()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "core.json");
        File.WriteAllText(path, "{\"value\":1}");
        using var service = new ConfigurationService(new TestLogger(), _directory);
        service.LoadConfiguration();
        File.WriteAllText(path, "invalid");

        Assert.Throws<InvalidDataException>(() => service.Reload());
        Assert.Equal(1, service.GetValue<int>("value"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }
}
