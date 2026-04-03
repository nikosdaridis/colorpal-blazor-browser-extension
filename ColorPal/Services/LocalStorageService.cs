using Blazored.LocalStorage;
using ColorPal.Common;
using ColorPal.Common.Models;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ColorPal.Services;

public sealed partial class LocalStorageService(ILocalStorageService LocalStorageService, IJSRuntime JSRuntime)
{
    [GeneratedRegex("^#[\\dabcdef]{6}$", RegexOptions.IgnoreCase)]
    private static partial Regex HexColorValidationRegex();

    /// <summary>
    /// Gets localstorage value for enum key
    /// </summary>
    public async Task<T?> GetKeyAsync<T>(StorageKey key) =>
        await LocalStorageService.GetItemAsync<T>(key.Value());

    /// <summary>
    /// Sets localstorage value for enum key
    /// </summary>
    public async Task SetKeyAsync<T>(StorageKey key, T value) =>
        await LocalStorageService.SetItemAsync(key.Value(), value);

    public async Task ValidateAsync()
    {
        // Version
        string version = await JSRuntime.InvokeAsync<string>(JsFuncs.GetManifestVersionAsync.Value()) ?? string.Empty;
        _ = SetKeyAsync(StorageKey.Version, version);

        // Theme
        string? storedTheme = await GetKeyAsync<string>(StorageKey.Theme);
        string theme = storedTheme is "light" or "dark"
            ? storedTheme
            : (await JSRuntime.InvokeAsync<string>(JsFuncs.GetClientColorScheme.Value()));
        _ = SetKeyAsync(StorageKey.Theme, theme);
        _ = SetThemeAsync(theme);

        // SelectedHexColor
        string storedSelectedHexColor = await GetKeyAsync<string>(StorageKey.SelectedHexColor) ?? "#000000";
        string selectedHexColor = HexColorValidationRegex().IsMatch(storedSelectedHexColor) ? storedSelectedHexColor : "#000000";
        _ = SetKeyAsync(StorageKey.SelectedHexColor, selectedHexColor);

        // SavedColorsArray
        List<string> storedSavedColorsArray = await ValidateJsonArrayAsync(StorageKey.SavedColorsArray, "[]");
        storedSavedColorsArray = [.. storedSavedColorsArray.Where(color => HexColorValidationRegex().IsMatch(color))];
        _ = SetKeyAsync(StorageKey.SavedColorsArray, storedSavedColorsArray);

        // AutoSaveEyedropper
        _ = ValidateTrueOrFalseAsync(StorageKey.AutoSaveEyedropper, "true");

        // AutoCopyCode
        _ = ValidateTrueOrFalseAsync(StorageKey.AutoCopyCode, "true");

        // ColorCodeFormat
        string? storedColorCodeFormat = await GetKeyAsync<string>(StorageKey.ColorCodeFormat);
        if (!EnumExtensions.TryParse(storedColorCodeFormat, out ColorCodeFormat codeFormatEnum))
        {
            codeFormatEnum = ColorCodeFormat.HEX;
        }
        _ = SetKeyAsync(StorageKey.ColorCodeFormat, codeFormatEnum.Value());

        // AddHexCharacter
        _ = ValidateTrueOrFalseAsync(StorageKey.AddHexCharacter, "true");

        // ColorsPerLine
        string storedColorsPerLine = await GetKeyAsync<string>(StorageKey.ColorsPerLine) ?? "5";
        _ = int.TryParse(storedColorsPerLine, out int colorsPerLine);
        colorsPerLine = colorsPerLine >= 5 && colorsPerLine <= 10 ? colorsPerLine : 5;
        _ = SetKeyAsync(StorageKey.ColorsPerLine, colorsPerLine.ToString());

        // ShowColorNames
        _ = ValidateTrueOrFalseAsync(StorageKey.ShowColorNames, "false");

        // PrependBlackFilter
        await ValidateTrueOrFalseAsync(StorageKey.PrependBlackFilter, "false");

        async Task<List<string>> ValidateJsonArrayAsync(StorageKey key, string fallbackValue)
        {
            try
            {
                string storedValue = await GetKeyAsync<string>(key) ?? string.Empty;
                return JsonSerializer.Deserialize<List<string>>(storedValue) ?? JsonSerializer.Deserialize<List<string>>(fallbackValue)!;
            }
            catch
            {
                await SetKeyAsync(key, fallbackValue);
                return JsonSerializer.Deserialize<List<string>>(fallbackValue)!;
            }
        }

        async Task ValidateTrueOrFalseAsync(StorageKey key, string defaultValue)
        {
            string? storedValue = await GetKeyAsync<string>(key);
            if (storedValue is not "true" and not "false")
            {
                _ = SetKeyAsync(key, defaultValue);
            }
        }
    }

    /// <summary>
    /// Converts a hex color to the specified format string.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime for interop calls.</param>
    /// <param name="hexColor">The hex color to convert.</param>
    /// <param name="format">The target color code format.</param>
    /// <returns>The formatted color code string and the CSS filter if applicable.</returns>
    public async Task<(string ColorCode, CssFilter? Filter)> GetFormattedColorCodeAsync(IJSRuntime jsRuntime, string hexColor, ColorCodeFormat format)
    {
        CssFilter? filter = null;

        if (format == ColorCodeFormat.Filter)
        {
            filter = await jsRuntime.InvokeAsync<CssFilter>(JsFuncs.HexToFilter.Value(), hexColor);

            if (await GetKeyAsync<string>(StorageKey.PrependBlackFilter) == "true")
            {
                filter.Filter = filter.Filter?.Replace("filter:", "filter: brightness(0) saturate(100%)");
            }
        }

        string colorCode = format switch
        {
            ColorCodeFormat.HEX => await GetKeyAsync<string>(StorageKey.AddHexCharacter) == "true" ? hexColor : hexColor[1..],
            ColorCodeFormat.RGB => await jsRuntime.InvokeAsync<string>(JsFuncs.HexToRgb.Value(), hexColor, "string"),
            ColorCodeFormat.HSL => await jsRuntime.InvokeAsync<string>(JsFuncs.HexToHsl.Value(), hexColor, "string"),
            ColorCodeFormat.HSV => await jsRuntime.InvokeAsync<string>(JsFuncs.HexToHsv.Value(), hexColor, "string"),
            ColorCodeFormat.Filter => filter?.Filter ?? hexColor,
            _ => hexColor
        };

        return (colorCode, filter);
    }

    /// <summary>
    /// Sets localstorage theme and updates css variables.
    /// </summary>
    public async Task SetThemeAsync(Theme theme, bool saveLocalStorage = true)
    {
        if (saveLocalStorage)
        {
            await SetKeyAsync(StorageKey.Theme, theme.Value());
        }

        await SetCssVariableAsync(CSSVariable.Primary, Color.LightPrimary, Color.DarkPrimary);
        await SetCssVariableAsync(CSSVariable.Secondary, Color.LightSecondary, Color.DarkSecondary);
        await SetCssVariableAsync(CSSVariable.Text, Color.LightText, Color.DarkText);
        await SetCssVariableAsync(CSSVariable.ThemeInvert, Color.LightThemeInvert, Color.DarkThemeInvert);

        // Sets theme filter based on the current theme
        await JSRuntime.InvokeVoidAsync("document.documentElement.style.setProperty", CSSVariable.ThemeFilter.Value(),
            await JSRuntime.InvokeAsync<string>(JsFuncs.GetThemeFilter.Value(), theme.Value()));

        // Sets css variable for theme
        async Task SetCssVariableAsync(CSSVariable property, Color light, Color dark) =>
            await JSRuntime.InvokeVoidAsync("document.documentElement.style.setProperty", property.Value(), theme == Theme.Light ? light.Value() : dark.Value());
    }

    /// <summary>
    /// Sets localstorage theme and updates css variables
    /// </summary>
    public async Task SetThemeAsync(string theme, bool saveLocalStorage = true) =>
        await SetThemeAsync(Enum.Parse<Theme>(theme, true), saveLocalStorage);
}
