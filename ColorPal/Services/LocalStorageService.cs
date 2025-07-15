using Blazored.LocalStorage;
using ColorPal.Common;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ColorPal.Services
{
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
            string theme = storedTheme == "light" || storedTheme == "dark"
                ? storedTheme
                : (await JSRuntime.InvokeAsync<string>(JsFuncs.GetClientColorScheme.Value()));
            _ = SetKeyAsync(StorageKey.Theme, theme);
            _ = SetThemeAsync(theme);

            // SelectedHexColor
            string storedSelectedHexColor = await GetKeyAsync<string>(StorageKey.SelectedHexColor) ?? "#000000";
            string selectedHexColor = HexColorValidationRegex().IsMatch(storedSelectedHexColor) ? storedSelectedHexColor : "#000000";
            _ = SetKeyAsync(StorageKey.SelectedHexColor, selectedHexColor);

            // SavedColorsArray
            List<string> storedsavedColorsArray = await ValidateJsonArrayAsync(StorageKey.SavedColorsArray, "[]");
            storedsavedColorsArray = storedsavedColorsArray.Where(color => HexColorValidationRegex().IsMatch(color)).ToList();
            _ = SetKeyAsync(StorageKey.SavedColorsArray, storedsavedColorsArray);

            // AutoSaveEyedropper
            _ = ValidateTrueOrFalseAsync(StorageKey.AutoSaveEyedropper, "true");

            // AutoCopyCode
            _ = ValidateTrueOrFalseAsync(StorageKey.AutoCopyCode, "true");

            // ColorCodeFormat
            string? storedColorCodeFormat = await GetKeyAsync<string>(StorageKey.ColorCodeFormat);
            if (!EnumExtensions.TryParse(storedColorCodeFormat, out ColorCodeFormat codeFormatEnum))
                codeFormatEnum = ColorCodeFormat.HEX;
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
                if (storedValue != "true" && storedValue != "false")
                    _ = SetKeyAsync(key, defaultValue);
            }
        }

        /// <summary>
        /// Sets localstorage theme and updates css variables
        /// </summary>
        public async Task SetThemeAsync(Theme theme, bool saveLocalStorage = true)
        {
            if (saveLocalStorage)
                await SetKeyAsync(StorageKey.Theme, theme.Value());

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
}
