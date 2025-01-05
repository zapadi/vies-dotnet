namespace Padi.Vies;

internal readonly struct ExcludedCountryInfo
{
    public ExcludedCountryInfo(string code, string name, string reason, string date)
    {
        Code = code;
        Name = name;
        Reason = reason;
        Date = date;
    }

    public string Code { get; }
    public string Name { get; }
    public string Reason { get; }
    public string Date { get; }

    public override string ToString()
    {
        return $"{Name}({Code}) is no longer supported by VIES services provided by EC since {Date} because of {Reason}";
    }
}
