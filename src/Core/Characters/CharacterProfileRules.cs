namespace ProjectLiberty.Core.Characters;

public static class CharacterProfileRules
{
    public const int MaximumCharacters = 3;

    public static void ValidateSlot(int slot)
    {
        if (slot < 1 || slot > MaximumCharacters)
            throw new ArgumentOutOfRangeException(nameof(slot), $"Character slot must be between 1 and {MaximumCharacters}.");
    }

    public static void Normalize(CharacterProfile profile)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));
        ValidateSlot(profile.Slot);
        profile.FirstName = NormalizeName(profile.FirstName, "Новый");
        profile.LastName = NormalizeName(profile.LastName, "Персонаж");
        profile.FatherId = Clamp(profile.FatherId, 0, 23);
        profile.MotherId = Clamp(profile.MotherId, 21, 45);
        profile.ShapeMix = Clamp(profile.ShapeMix, 0f, 1f);
        profile.SkinMix = Clamp(profile.SkinMix, 0f, 1f);
        profile.EyeColor = Clamp(profile.EyeColor, 0, 31);
        profile.HairDrawable = Math.Max(0, profile.HairDrawable);
        profile.HairTexture = Math.Max(0, profile.HairTexture);
        profile.HairColor = Clamp(profile.HairColor, 0, 63);
        profile.HairHighlightColor = Clamp(profile.HairHighlightColor, 0, 63);

        if (profile.FaceFeatures == null || profile.FaceFeatures.Length != 20)
            profile.FaceFeatures = new float[20];
        for (var i = 0; i < profile.FaceFeatures.Length; i++)
            profile.FaceFeatures[i] = Clamp(profile.FaceFeatures[i], -1f, 1f);

        if (profile.Overlays == null || profile.Overlays.Length != 13)
            profile.Overlays = CharacterProfile.CreateDefault(profile.Slot).Overlays;
        for (var i = 0; i < profile.Overlays.Length; i++)
        {
            profile.Overlays[i] ??= new CharacterOverlay();
            profile.Overlays[i].OverlayId = i;
            profile.Overlays[i].Value = Math.Max(-1, profile.Overlays[i].Value);
            profile.Overlays[i].Opacity = Clamp(profile.Overlays[i].Opacity, 0f, 1f);
            profile.Overlays[i].ColorType = Clamp(profile.Overlays[i].ColorType, 0, 2);
            profile.Overlays[i].Color = Clamp(profile.Overlays[i].Color, 0, 63);
            profile.Overlays[i].SecondaryColor = Clamp(profile.Overlays[i].SecondaryColor, 0, 63);
        }

        if (profile.Components == null || profile.Components.Length != 12)
            profile.Components = CharacterProfile.CreateDefault(profile.Slot).Components;
        for (var i = 0; i < profile.Components.Length; i++)
        {
            profile.Components[i] ??= new CharacterComponent();
            profile.Components[i].ComponentId = i;
            profile.Components[i].Drawable = Math.Max(0, profile.Components[i].Drawable);
            profile.Components[i].Texture = Math.Max(0, profile.Components[i].Texture);
            profile.Components[i].Palette = Clamp(profile.Components[i].Palette, 0, 3);
        }

        if (profile.Props == null || profile.Props.Length != 8)
            profile.Props = CharacterProfile.CreateDefault(profile.Slot).Props;
        for (var i = 0; i < profile.Props.Length; i++)
        {
            profile.Props[i] ??= new CharacterProp();
            profile.Props[i].PropId = i;
            profile.Props[i].Drawable = Math.Max(-1, profile.Props[i].Drawable);
            profile.Props[i].Texture = Math.Max(0, profile.Props[i].Texture);
        }
    }

    private static string NormalizeName(string? value, string fallback)
    {
        var cleaned = (value ?? string.Empty).Trim();
        if (cleaned.Length == 0) return fallback;
        return cleaned.Length <= 20 ? cleaned : cleaned.Substring(0, 20);
    }

    private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    private static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
}
