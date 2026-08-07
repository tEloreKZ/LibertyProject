using GTA;
using GTA.Native;
using ProjectLiberty.Core.Characters;

namespace ProjectLiberty.LibertyScript.Characters;

internal static class GtaCharacterAppearance
{
    private const ulong SetPedFaceFeature = 0x71A5C1DBA060049E;

    public static bool Apply(CharacterProfile profile)
    {
        CharacterProfileRules.Normalize(profile);
        var model = new Model(profile.Gender == CharacterGender.Male ? "mp_m_freemode_01" : "mp_f_freemode_01");
        if (!model.IsInCdImage || !model.IsPed) return false;
        if (Game.Player.Character.Model.Hash != model.Hash && (!model.Request(3000) || !Game.Player.ChangeModel(model))) return false;

        var ped = Game.Player.Character;
        if (ped.Model.Hash != model.Hash) return false;
        Function.Call(Hash.SET_PED_DEFAULT_COMPONENT_VARIATION, ped.Handle);
        ApplyHeadBlend(ped, profile);

        for (var i = 0; i < profile.FaceFeatures.Length; i++)
            Function.Call((Hash)SetPedFaceFeature, ped.Handle, i, profile.FaceFeatures[i]);

        Function.Call(Hash.SET_HEAD_BLEND_EYE_COLOR, ped.Handle, profile.EyeColor);
        Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ped.Handle, 2,
            SafeDrawable(ped, 2, profile.HairDrawable),
            SafeTexture(ped, 2, profile.HairDrawable, profile.HairTexture), 0);
        Function.Call(Hash.SET_PED_HAIR_TINT, ped.Handle, profile.HairColor, profile.HairHighlightColor);

        foreach (var overlay in profile.Overlays)
        {
            var maximum = Math.Max(0, Function.Call<int>(Hash.GET_PED_HEAD_OVERLAY_NUM, overlay.OverlayId));
            var value = overlay.Value < 0 ? 255 : Math.Min(overlay.Value, Math.Max(0, maximum - 1));
            Function.Call(Hash.SET_PED_HEAD_OVERLAY, ped.Handle, overlay.OverlayId, value, overlay.Opacity);
            if (value != 255)
                Function.Call(Hash.SET_PED_HEAD_OVERLAY_TINT, ped.Handle, overlay.OverlayId,
                    overlay.ColorType, overlay.Color, overlay.SecondaryColor);
        }

        foreach (var component in profile.Components)
        {
            if (component.ComponentId == 2 || !component.IsSet) continue;
            var drawable = SafeDrawable(ped, component.ComponentId, component.Drawable);
            var texture = SafeTexture(ped, component.ComponentId, drawable, component.Texture);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ped.Handle, component.ComponentId, drawable, texture, component.Palette);
        }

        foreach (var prop in profile.Props)
        {
            if (!prop.IsSet) continue;
            if (prop.Drawable < 0)
            {
                Function.Call(Hash.CLEAR_PED_PROP, ped.Handle, prop.PropId);
                continue;
            }
            var drawables = Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_DRAWABLE_VARIATIONS, ped.Handle, prop.PropId);
            if (drawables <= 0) continue;
            var drawable = Math.Min(prop.Drawable, drawables - 1);
            var textures = Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, ped.Handle, prop.PropId, drawable);
            Function.Call(Hash.SET_PED_PROP_INDEX, ped.Handle, prop.PropId, drawable,
                textures <= 0 ? 0 : Math.Min(prop.Texture, textures - 1), true);
        }

        return true;
    }

    public static void ApplyContinuous(CharacterProfile profile, int appearanceIndex)
    {
        var ped = Game.Player.Character;
        var expectedModel = new Model(profile.Gender == CharacterGender.Male ? "mp_m_freemode_01" : "mp_f_freemode_01");
        if (ped.Model.Hash != expectedModel.Hash)
        {
            Apply(profile);
            return;
        }

        if (appearanceIndex is 3 or 4) ApplyHeadBlend(ped, profile);
        else if (appearanceIndex >= 8 && appearanceIndex < 28)
            Function.Call((Hash)SetPedFaceFeature, ped.Handle, appearanceIndex - 8, profile.FaceFeatures[appearanceIndex - 8]);
    }

    public static int DrawableCount(Ped ped, int componentId) =>
        Math.Max(1, Function.Call<int>(Hash.GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS, ped.Handle, componentId));

    public static int TextureCount(Ped ped, int componentId, int drawable) =>
        Math.Max(1, Function.Call<int>(Hash.GET_NUMBER_OF_PED_TEXTURE_VARIATIONS, ped.Handle, componentId, drawable));

    public static int PropDrawableCount(Ped ped, int propId) =>
        Math.Max(0, Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_DRAWABLE_VARIATIONS, ped.Handle, propId));

    private static void ApplyHeadBlend(Ped ped, CharacterProfile profile) =>
        Function.Call(Hash.SET_PED_HEAD_BLEND_DATA, ped.Handle,
            profile.FatherId, profile.MotherId, 0,
            profile.FatherId, profile.MotherId, 0,
            profile.ShapeMix, profile.SkinMix, 0f, false);

    private static int SafeDrawable(Ped ped, int componentId, int drawable) =>
        Math.Min(Math.Max(0, drawable), DrawableCount(ped, componentId) - 1);

    private static int SafeTexture(Ped ped, int componentId, int drawable, int texture) =>
        Math.Min(Math.Max(0, texture), TextureCount(ped, componentId, SafeDrawable(ped, componentId, drawable)) - 1);
}
