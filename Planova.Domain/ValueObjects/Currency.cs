namespace Planova.Domain.ValueObjects;

public sealed class Currency
{
    public static readonly Currency Usd = new("USD");
    public static readonly Currency Eur = new("EUR");
    public static readonly Currency Gbp = new("GBP");
    public static readonly Currency Egp = new("EGP");
    public static readonly Currency Sar = new("SAR");
    public static readonly Currency Aed = new("AED");

    public string Code { get; }

    private Currency(string code)
    {
        Code = code;
    }

    public static Currency FromCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.ToUpperInvariant();
        return normalized switch
        {
            "USD" => Usd,
            "EUR" => Eur,
            "GBP" => Gbp,
            "EGP" => Egp,
            "SAR" => Sar,
            "AED" => Aed,
            _ => new Currency(normalized),
        };
    }

    public override string ToString() => Code;

    public override bool Equals(object? obj) =>
        obj is Currency other && Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode();

    public static bool operator ==(Currency? left, Currency? right) =>
        Equals(left, right);

    public static bool operator !=(Currency? left, Currency? right) =>
        !Equals(left, right);
}
