using System.Buffers;
using System.Text;

namespace ColorPal.Common;

/// <summary>
/// Writes CSV rows  with quoted cells.
/// </summary>
public static class CsvFormatter
{
    private const char CELL_SEPARATOR = ',';
    private const char QUOTE = '"';
    private const string QUOTE_TEXT = "\"";
    private const string ESCAPED_QUOTE_TEXT = "\"\"";
    private const char FORMULA_NEUTRALIZING_PREFIX = '\'';

    private static readonly SearchValues<char> _formulaTriggerCharacters = SearchValues.Create("=+-@\t\r");

    public static void AppendRow(StringBuilder csvBuilder, ReadOnlySpan<string> cellValues)
    {
        for (int cellIndex = 0; cellIndex < cellValues.Length; cellIndex++)
        {
            if (cellIndex > 0)
            {
                csvBuilder.Append(CELL_SEPARATOR);
            }

            AppendCell(csvBuilder, cellValues[cellIndex]);
        }

        csvBuilder.AppendLine();
    }

    private static void AppendCell(StringBuilder csvBuilder, string cellValue)
    {
        csvBuilder.Append(QUOTE);

        if (cellValue.Length > 0 && _formulaTriggerCharacters.Contains(cellValue[0]))
        {
            csvBuilder.Append(FORMULA_NEUTRALIZING_PREFIX);
        }

        csvBuilder.Append(cellValue.Replace(QUOTE_TEXT, ESCAPED_QUOTE_TEXT)).Append(QUOTE);
    }
}
