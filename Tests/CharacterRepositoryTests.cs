using ProjectLiberty.Core.Characters;
using Xunit;

namespace ProjectLiberty.Tests;

public sealed class CharacterRepositoryTests
{
    [Fact]
    public void Save_RoundTripsProfileAndActiveSlot()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ProjectLibertyTests", Guid.NewGuid().ToString("N"));
        try
        {
            var path = Path.Combine(directory, "characters.json");
            var repository = new CharacterRepository(path);
            var profile = CharacterProfile.CreateDefault(2);
            profile.FirstName = "John";
            profile.LastName = "Roman";
            profile.FaceFeatures[3] = .7f;
            profile.Components[11].Drawable = 4;
            profile.Components[11].IsSet = true;
            repository.Save(profile);

            var reloaded = new CharacterRepository(path);
            var saved = reloaded.Get(2);

            Assert.NotNull(saved);
            Assert.Equal("John Roman", saved!.FullName);
            Assert.Equal(.7f, saved.FaceFeatures[3]);
            Assert.Equal(4, saved.Components[11].Drawable);
            Assert.True(saved.Components[11].IsSet);
            Assert.False(saved.Components[1].IsSet);
            Assert.Equal(2, reloaded.ActiveSlot);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Save_EnforcesThreeSlotsAndReportsNoFreeSlot()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ProjectLibertyTests", Guid.NewGuid().ToString("N"));
        try
        {
            var repository = new CharacterRepository(Path.Combine(directory, "characters.json"));
            repository.Save(CharacterProfile.CreateDefault(1));
            repository.Save(CharacterProfile.CreateDefault(2));
            repository.Save(CharacterProfile.CreateDefault(3));

            Assert.Equal(3, repository.Count);
            Assert.Equal(0, repository.FirstFreeSlot());
            Assert.Throws<ArgumentOutOfRangeException>(() => repository.Save(CharacterProfile.CreateDefault(4)));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}
