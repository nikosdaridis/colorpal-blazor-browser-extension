namespace ColorPal.Common.Models;

public sealed record ColorHSL
{
    public int H { get; init; }
    public int S { get; init; }
    public int L { get; init; }
}
