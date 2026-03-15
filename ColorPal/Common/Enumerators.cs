namespace ColorPal.Common;

public enum Event
{
    EyedropperPick,
    OpenColorPicker,
    SetSelectedColor,
    SaveColor,
    ColorCodeActiveTab,
    ColorsPerLine,
    SetActiveColorTool,
    RenderTintsShades,
    DeleteSavedColor,
    DeleteAllSavedColors
}

public enum ColorToolType
{
    [Value("")]
    None,

    [Value("move")]
    Move,

    [Value("tintsShades")]
    TintsShades,

    [Value("delete")]
    Delete,

    [Value("deleteall")]
    DeleteAll,

    [Value("downloadpng")]
    DownloadPNG,

    [Value("downloadcsv")]
    DownloadCSV,
}

public enum ColorCodeFormat
{
    [Value("")]
    None,

    [Value("HEX")]
    HEX,

    [Value("RGB")]
    RGB,

    [Value("HSL")]
    HSL,

    [Value("HSV")]
    HSV,

    [Value("Filter")]
    Filter,
}

public enum StorageKey
{
    [Value("colorpal-version")]
    Version,

    [Value("colorpal-theme")]
    Theme,

    [Value("colorpal-selected-color")]
    SelectedHexColor,

    [Value("colorpal-saved-colors-array")]
    SavedColorsArray,

    [Value("colorpal-auto-save-eyedropper")]
    AutoSaveEyedropper,

    [Value("colorpal-auto-copy-code")]
    AutoCopyCode,

    [Value("colorpal-color-code-format")]
    ColorCodeFormat,

    [Value("colorpal-add-hex-character")]
    AddHexCharacter,

    [Value("colorpal-colors-per-line")]
    ColorsPerLine,

    [Value("colorpal-show-color-names")]
    ShowColorNames,

    [Value("colorpal-prepend-black-filter")]
    PrependBlackFilter,
}

public enum JsFuncs
{
    [Value("isInitialized")]
    IsInitialized,

    [Value("initializeStateService")]
    InitializeStateService,

    [Value("initializeColorsGridComponent")]
    InitializeColorsGridComponent,

    [Value("getClientColorScheme")]
    GetClientColorScheme,

    [Value("getThemeFilter")]
    GetThemeFilter,

    [Value("getManifestVersionAsync")]
    GetManifestVersionAsync,

    [Value("setColorPickerPosition")]
    SetColorPickerPosition,

    [Value("setSelectValue")]
    SetSelectValue,

    [Value("setGridColumns")]
    SetGridColumns,

    [Value("activateEyeDropper")]
    ActivateEyeDropper,

    [Value("openColorPicker")]
    OpenColorPicker,

    [Value("copyToClipboard")]
    CopyToClipboard,

    [Value("setElementValueById")]
    SetElementValueById,

    [Value("downloadPng")]
    DownloadPng,

    [Value("downloadCsv")]
    DownloadCsv,

    [Value("handleColorsMove")]
    HandleColorsMove,

    [Value("removeMoveColorsListeners")]
    RemoveMoveColorsListeners,

    //Color Converters
    [Value("hexToRgb")]
    HexToRgb,

    [Value("hexToHsl")]
    HexToHsl,

    [Value("hexToHsv")]
    HexToHsv,

    [Value("rgbToHex")]
    RgbToHex,

    [Value("rgbToHsl")]
    RgbToHsl,

    [Value("rgbToHsv")]
    RgbToHsv,

    [Value("hslToHex")]
    HslToHex,

    [Value("hexToFilter")]
    HexToFilter
}

public enum Theme
{
    [Value("light")]
    Light,

    [Value("dark")]
    Dark
}

public enum CSSVariable
{
    [Value("--primary-color")]
    Primary,

    [Value("--secondary-color")]
    Secondary,

    [Value("--text-color")]
    Text,

    [Value("--theme-invert-color")]
    ThemeInvert,

    [Value("--theme-filter")]
    ThemeFilter,
}

public enum Color
{
    [Value("#ffffff")]
    LightPrimary,

    [Value("#09090b")]
    DarkPrimary,

    [Value("#f4f4f5")]
    LightSecondary,

    [Value("#27272a")]
    DarkSecondary,

    [Value("#787881")]
    LightText,

    [Value("#9f9fa8")]
    DarkText,

    [Value("#0a0a0a")]
    LightThemeInvert,

    [Value("#fafafa")]
    DarkThemeInvert,
}

public enum Size
{
    Small,
    Medium,
    Large
}
