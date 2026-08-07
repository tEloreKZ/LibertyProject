namespace ProjectLiberty.Core.Characters;

public enum CharacterGender
{
    Male,
    Female
}

public sealed class CharacterProfile
{
    public int Slot { get; set; }
    public string FirstName { get; set; } = "Новый";
    public string LastName { get; set; } = "Персонаж";
    public CharacterGender Gender { get; set; } = CharacterGender.Male;
    public int FatherId { get; set; } = 0;
    public int MotherId { get; set; } = 21;
    public float ShapeMix { get; set; } = 0.5f;
    public float SkinMix { get; set; } = 0.5f;
    public int EyeColor { get; set; } = 0;
    public int HairDrawable { get; set; } = 0;
    public int HairTexture { get; set; } = 0;
    public int HairColor { get; set; } = 0;
    public int HairHighlightColor { get; set; } = 0;
    public float[] FaceFeatures { get; set; } = new float[20];
    public CharacterOverlay[] Overlays { get; set; } = CreateOverlays();
    public CharacterComponent[] Components { get; set; } = CreateComponents();
    public CharacterProp[] Props { get; set; } = CreateProps();
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public static CharacterProfile CreateDefault(int slot)
    {
        CharacterProfileRules.ValidateSlot(slot);
        return new CharacterProfile { Slot = slot };
    }

    private static CharacterComponent[] CreateComponents()
    {
        var result = new CharacterComponent[12];
        for (var i = 0; i < result.Length; i++) result[i] = new CharacterComponent { ComponentId = i };
        return result;
    }

    private static CharacterProp[] CreateProps()
    {
        var result = new CharacterProp[8];
        for (var i = 0; i < result.Length; i++) result[i] = new CharacterProp { PropId = i, Drawable = -1 };
        return result;
    }

    private static CharacterOverlay[] CreateOverlays()
    {
        var result = new CharacterOverlay[13];
        for (var i = 0; i < result.Length; i++) result[i] = new CharacterOverlay { OverlayId = i };
        return result;
    }
}

public sealed class CharacterComponent
{
    public int ComponentId { get; set; }
    public int Drawable { get; set; }
    public int Texture { get; set; }
    public int Palette { get; set; }
    public bool IsSet { get; set; }
}

public sealed class CharacterProp
{
    public int PropId { get; set; }
    public int Drawable { get; set; } = -1;
    public int Texture { get; set; }
    public bool IsSet { get; set; }
}

public sealed class CharacterOverlay
{
    public int OverlayId { get; set; }
    public int Value { get; set; } = -1;
    public float Opacity { get; set; } = 1f;
    public int ColorType { get; set; } = 1;
    public int Color { get; set; }
    public int SecondaryColor { get; set; }
}

public sealed class CharacterSaveData
{
    public int Version { get; set; } = 1;
    public int? ActiveSlot { get; set; }
    public List<CharacterProfile> Characters { get; set; } = new List<CharacterProfile>();
}
