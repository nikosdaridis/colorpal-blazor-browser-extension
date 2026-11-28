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

    [Value("colorpal-language")]
    Language,

    [Value("colorpal-eyedropper-pick-count")]
    EyedropperPickCount,

    [Value("colorpal-rating-open-count")]
    RatingOpenCount,

    [Value("colorpal-rating-open-days")]
    RatingOpenDays,

    [Value("colorpal-rating-last-open-day")]
    RatingLastOpenDay,

    [Value("colorpal-rating-prompts-left")]
    RatingPromptsLeft,

    [Value("colorpal-rating-last-shown-day")]
    RatingLastShownDay,
}

public enum MessageKey
{
    [Value("eyedropper")]
    Eyedropper,

    [Value("copyColorName")]
    CopyColorName,

    [Value("settings")]
    Settings,

    [Value("saveColor")]
    SaveColor,

    [Value("selectedColor")]
    SelectedColor,

    [Value("filterFormat")]
    FilterFormat,

    [Value("copyColorCode")]
    CopyColorCode,

    [Value("moveColors")]
    MoveColors,

    [Value("tintsAndShades")]
    TintsAndShades,

    [Value("deleteColors")]
    DeleteColors,

    [Value("deleteAllColors")]
    DeleteAllColors,

    [Value("downloadPng")]
    DownloadPng,

    [Value("downloadCsv")]
    DownloadCsv,

    [Value("colorsPerLine")]
    ColorsPerLine,

    [Value("deleteAllConfirmation")]
    DeleteAllConfirmation,

    [Value("back")]
    Back,

    [Value("theme")]
    Theme,

    [Value("darkTheme")]
    DarkTheme,

    [Value("language")]
    Language,

    [Value("automaticLanguage")]
    AutomaticLanguage,

    [Value("autoSaveEyedropper")]
    AutoSaveEyedropper,

    [Value("autoCopyCode")]
    AutoCopyCode,

    [Value("addHashToHex")]
    AddHashToHex,

    [Value("showColorName")]
    ShowColorName,

    [Value("prependBlackToFilter")]
    PrependBlackToFilter,

    [Value("website")]
    Website,

    [Value("version")]
    Version,

    [Value("ratingReminderTitle")]
    RatingReminderTitle,

    [Value("ratingReminder")]
    RatingReminder,

    [Value("notNow")]
    NotNow,

    [Value("rateColorPal")]
    RateColorPal,
}

public enum JsFunctions
{
    [Value("isInitialized")]
    IsInitialized,

    [Value("initializeStateService")]
    InitializeStateService,

    [Value("initializeColorsGridComponent")]
    InitializeColorsGridComponent,

    [Value("disposeColorsGridComponent")]
    DisposeColorsGridComponent,

    [Value("getClientColorScheme")]
    GetClientColorScheme,

    [Value("scheduleBootSnapshotSave")]
    ScheduleBootSnapshotSave,

    [Value("getThemeFilter")]
    GetThemeFilter,

    [Value("setCssVariable")]
    SetCssVariable,

    [Value("getManifestVersionAsync")]
    GetManifestVersionAsync,

    [Value("getLocalizedMessagesAsync")]
    GetLocalizedMessagesAsync,

    [Value("getLocalStorageItem")]
    GetLocalStorageItem,

    [Value("getLocalStorageItems")]
    GetLocalStorageItems,

    [Value("setLocalStorageItem")]
    SetLocalStorageItem,

    [Value("openExternalUrl")]
    OpenExternalUrl,

    [Value("setColorPickerPosition")]
    SetColorPickerPosition,

    [Value("setSelectValue")]
    SetSelectValue,

    [Value("setGridColumns")]
    SetGridColumns,

    [Value("restoreLostFocus")]
    RestoreLostFocus,

    [Value("revealRatingReminder")]
    RevealRatingReminder,

    [Value("activateEyeDropper")]
    ActivateEyeDropper,

    [Value("openColorPicker")]
    OpenColorPicker,

    [Value("copyToClipboard")]
    CopyToClipboard,

    [Value("confirmAction")]
    ConfirmAction,

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

    [Value("rgbToHsl")]
    RgbToHsl,

    [Value("rgbToHsv")]
    RgbToHsv,

    [Value("hslToHexRange")]
    HslToHexRange,

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
    [Value("var(--light-primary-color)")]
    LightPrimary,

    [Value("var(--dark-primary-color)")]
    DarkPrimary,

    [Value("var(--light-secondary-color)")]
    LightSecondary,

    [Value("var(--dark-secondary-color)")]
    DarkSecondary,

    [Value("var(--light-text-color)")]
    LightText,

    [Value("var(--dark-text-color)")]
    DarkText,

    [Value("var(--light-theme-invert-color)")]
    LightThemeInvert,

    [Value("var(--dark-theme-invert-color)")]
    DarkThemeInvert,
}

public enum Size
{
    Small,
    Medium
}
