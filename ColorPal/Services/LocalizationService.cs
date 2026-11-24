using ColorPal.Common;

namespace ColorPal.Services;

public sealed class LocalizationService(JsInterop jsInterop)
{
    private const string SUBSTITUTION_MARKER = "{0}";
    private static readonly MessageKey[] _messageKeys = Enum.GetValues<MessageKey>();
    private readonly Dictionary<MessageKey, string> _messageMap = [];

    public async Task LoadMessagesAsync()
    {
        string[] messageNames = Array.ConvertAll(_messageKeys, messageKey => messageKey.Value());
        string[] messages = await jsInterop.InvokeAsync<string[]>(JsFunctions.GetLocalizedMessagesAsync, messageNames, SUBSTITUTION_MARKER);

        for (int index = 0; index < _messageKeys.Length; index++)
        {
            _messageMap[_messageKeys[index]] = messages[index];
        }
    }

    public string GetMessage(MessageKey messageKey) =>
        _messageMap[messageKey];

    public string FormatMessage(MessageKey messageKey, string substitution) =>
        GetMessage(messageKey).Replace(SUBSTITUTION_MARKER, substitution);
}
