namespace ProjectLiberty.Abstractions.Localization;

/// <summary>
/// Exposes a read-only localization snapshot for UI clients such as CEF.
/// Kept separate from <see cref="ILocalization"/> so existing third-party
/// implementations of the public localization service remain compatible.
/// </summary>
public interface ILocalizationCatalog
{
    IReadOnlyDictionary<string, string> GetStrings(string prefix = "");
}
