using ColorPal.Common;
using System.Globalization;

namespace ColorPal.Services;

public sealed class RatingReminderService(LocalStorageService localStorageService, JsInterop jsInterop)
{
    public const int REQUIRED_OPEN_COUNT = 10;
    public const int REQUIRED_OPEN_DAY_COUNT = 7;
    public const int REQUIRED_PICK_COUNT = 10;
    public const int PROMPT_COUNT = 3;
    private const int DAYS_BETWEEN_PROMPTS = 30;
    private const int DAYS_AWAY_TO_RESTART_COUNTS = 30;
    private const string STORE_REVIEWS_URL = "https://chromewebstore.google.com/detail/mbnpegpimodgjmlbfhkkdgbcfjmgpoad/reviews";

    public static StorageKey[] StorageKeys { get; } =
    [
        StorageKey.EyedropperPickCount,
        StorageKey.RatingOpenCount,
        StorageKey.RatingOpenDays,
        StorageKey.RatingLastOpenDay,
        StorageKey.RatingPromptsLeft,
        StorageKey.RatingLastShownDay,
    ];

    private bool _isOpenRecorded;
    private int _pickCount;
    private int _openCount;
    private int _openDayCount;
    private int _promptsLeft;
    private int _lastShownDay;

    public bool IsRatingAvailable =>
        _pickCount >= REQUIRED_PICK_COUNT
        && _openCount >= REQUIRED_OPEN_COUNT
        && _openDayCount >= REQUIRED_OPEN_DAY_COUNT;

    public bool IsBannerVisible { get; private set; }

    public bool IsBannerDue =>
        IsRatingAvailable
        && _promptsLeft > 0
        && GetTodayDayNumber() - _lastShownDay >= DAYS_BETWEEN_PROMPTS;

    public async Task RecordOpenAsync()
    {
        if (_isOpenRecorded)
        {
            return;
        }

        _isOpenRecorded = true;
        Dictionary<StorageKey, string?> storedValueMap = await localStorageService.GetKeysAsync(StorageKeys);
        int today = GetTodayDayNumber();
        int lastOpenDay = ParseInteger(storedValueMap[StorageKey.RatingLastOpenDay], 0);
        bool isBackFromBreak = lastOpenDay > 0 && today - lastOpenDay >= DAYS_AWAY_TO_RESTART_COUNTS;
        _openCount = isBackFromBreak ? 0 : ParseInteger(storedValueMap[StorageKey.RatingOpenCount], 0);
        _openDayCount = isBackFromBreak ? 0 : ParseInteger(storedValueMap[StorageKey.RatingOpenDays], 0);

        if (_openCount < REQUIRED_OPEN_COUNT)
        {
            _openCount++;
            await SetIntegerAsync(StorageKey.RatingOpenCount, _openCount);
        }

        if (today > lastOpenDay)
        {
            if (_openDayCount < REQUIRED_OPEN_DAY_COUNT)
            {
                _openDayCount++;
                await SetIntegerAsync(StorageKey.RatingOpenDays, _openDayCount);
            }

            await SetIntegerAsync(StorageKey.RatingLastOpenDay, today);
        }

        _pickCount = ParseInteger(storedValueMap[StorageKey.EyedropperPickCount], 0);
        _promptsLeft = ParseInteger(storedValueMap[StorageKey.RatingPromptsLeft], PROMPT_COUNT);
        _lastShownDay = ParseInteger(storedValueMap[StorageKey.RatingLastShownDay], 0);
    }

    public async Task RecordEyedropperPickAsync(string hexColor)
    {
        if (_pickCount >= REQUIRED_PICK_COUNT || string.IsNullOrEmpty(hexColor))
        {
            return;
        }

        _pickCount++;
        await SetIntegerAsync(StorageKey.EyedropperPickCount, _pickCount);
        await jsInterop.InvokeVoidAsync(JsFunctions.ScheduleBootSnapshotSave);
    }

    public async Task ShowBannerAsync()
    {
        _promptsLeft = Math.Max(_promptsLeft - 1, 0);
        _lastShownDay = GetTodayDayNumber();
        await SetIntegerAsync(StorageKey.RatingPromptsLeft, _promptsLeft);
        await SetIntegerAsync(StorageKey.RatingLastShownDay, _lastShownDay);
        IsBannerVisible = true;
    }

    public void CloseBanner() =>
        IsBannerVisible = false;

    public async Task StopRemindingAsync()
    {
        _promptsLeft = 0;
        await SetIntegerAsync(StorageKey.RatingPromptsLeft, _promptsLeft);
        IsBannerVisible = false;
    }

    public async Task OpenStoreReviewsAsync() =>
        await jsInterop.InvokeVoidAsync(JsFunctions.OpenExternalUrl, STORE_REVIEWS_URL);

    public static bool TryParseInteger(string? value, out int number) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out number);

    private static int ParseInteger(string? value, int defaultValue) =>
        TryParseInteger(value, out int number) ? number : defaultValue;

    private async Task SetIntegerAsync(StorageKey key, int value) =>
        await localStorageService.SetKeyAsync(key, value.ToString(CultureInfo.InvariantCulture));

    private static int GetTodayDayNumber() =>
        DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
}
