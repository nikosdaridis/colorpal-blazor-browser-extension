using ColorPal.Common;
using ColorPal.Common.Models;
using System.Diagnostics.CodeAnalysis;

namespace ColorPal.Services;

public sealed class LocalizationService(JsInterop jsInterop, LocalStorageService localStorageService)
{
    public const string AUTOMATIC_LANGUAGE = "automatic";
    private const string SUBSTITUTION_MARKER = "{0}";
    private static readonly MessageKey[] _messageKeys = Enum.GetValues<MessageKey>();
    private readonly Dictionary<MessageKey, string> _messageMap = [];

    public static LanguageOption[] LanguageOptions { get; } =
    [
        new("id", "Bahasa Indonesia"),
        new("ms", "Bahasa Melayu"),
        new("ca", "Català"),
        new("cs", "Čeština"),
        new("da", "Dansk"),
        new("de", "Deutsch"),
        new("et", "Eesti"),
        new("en-GB", "English (UK)"),
        new("en", "English (US)"),
        new("es", "Español (España)"),
        new("es-419", "Español (Latinoamérica)"),
        new("fil", "Filipino"),
        new("fr", "Français"),
        new("hr", "Hrvatski"),
        new("it", "Italiano"),
        new("sw", "Kiswahili"),
        new("lv", "Latviešu"),
        new("lt", "Lietuvių"),
        new("hu", "Magyar"),
        new("nl", "Nederlands"),
        new("nb", "Norsk bokmål"),
        new("pl", "Polski"),
        new("pt-BR", "Português (Brasil)"),
        new("pt-PT", "Português (Portugal)"),
        new("ro", "Română"),
        new("sk", "Slovenčina"),
        new("sl", "Slovenščina"),
        new("fi", "Suomi"),
        new("sv", "Svenska"),
        new("vi", "Tiếng Việt"),
        new("tr", "Türkçe"),
        new("el", "Ελληνικά"),
        new("bg", "Български"),
        new("ru", "Русский"),
        new("sr", "Српски"),
        new("uk", "Українська"),
        new("he", "עברית"),
        new("ar", "العربية"),
        new("fa", "فارسی"),
        new("am", "አማርኛ"),
        new("mr", "मराठी"),
        new("hi", "हिन्दी"),
        new("bn", "বাংলা"),
        new("gu", "ગુજરાતી"),
        new("ta", "தமிழ்"),
        new("te", "తెలుగు"),
        new("kn", "ಕನ್ನಡ"),
        new("ml", "മലയാളം"),
        new("th", "ไทย"),
        new("ko", "한국어"),
        new("ja", "日本語"),
        new("zh-CN", "中文（简体）"),
        new("zh-TW", "中文（繁體）"),
    ];

    private static readonly HashSet<string> _languageCodes = [.. Array.ConvertAll(LanguageOptions, languageOption => languageOption.Code)];

    public string SelectedLanguage =>
        localStorageService.SelectedLanguage;

    public static bool IsSupportedLanguage([NotNullWhen(true)] string? language) =>
        language is AUTOMATIC_LANGUAGE || (language is not null && _languageCodes.Contains(language));

    public async Task LoadMessagesAsync()
    {
        string[] messageNames = Array.ConvertAll(_messageKeys, messageKey => messageKey.Value());
        string[] messages = await jsInterop.InvokeAsync<string[]>(JsFunctions.GetLocalizedMessagesAsync, messageNames, SUBSTITUTION_MARKER);

        for (int index = 0; index < _messageKeys.Length; index++)
        {
            _messageMap[_messageKeys[index]] = messages[index];
        }
    }

    public async Task SelectLanguageAsync(string language)
    {
        if (!IsSupportedLanguage(language) || language == SelectedLanguage)
        {
            return;
        }

        await localStorageService.SetSelectedLanguageAsync(language);
        await LoadMessagesAsync();
    }

    public string GetMessage(MessageKey messageKey) =>
        _messageMap[messageKey];

    public string FormatMessage(MessageKey messageKey, string substitution) =>
        GetMessage(messageKey).Replace(SUBSTITUTION_MARKER, substitution);
}
