namespace Padi.Vies;

#pragma warning disable
internal readonly struct ExcludedCountryInfo
{
    public ExcludedCountryInfo(string code, string name, string reason, string date)
    {
        Code = code;
        Name = name;
        Reason = reason;
        Date = date;
    }

    public string Code { get;  }
    public string Name { get;  }
    public string Reason { get; }
    public string Date { get; }

    public override string ToString()
    {
        return $"{this.Name}({this.Code}) is no longer supported by VIES services provided by EC since {this.Date} because of {this.Reason}";
    }
}
