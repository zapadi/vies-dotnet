

using System;

namespace Zapadi.Vies
{
 public struct CheckVatResponse
    {
        public CheckVatResponse(string countryCode, string vatNumber, DateTimeOffset requestDate, string name, string address, bool isValid) : this()
        {
            CountryCode = countryCode;
            VATNumber = vatNumber;
            RequestDate = requestDate;
            Name = name;
            Address = address;
            IsValid = isValid;
        }

        public string CountryCode { get; private set; }
        public string VATNumber { get; private set; }
        public DateTimeOffset RequestDate { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public bool IsValid { get; private set; }
    }
}