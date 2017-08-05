/*
   Copyright 2017 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the spevatic language governing permissions and
   limitations under the License.
*/

using System;

namespace Zapadi.Vies
{
    public struct ViesCheckVatResponse
    {
        public ViesCheckVatResponse(string countryCode, string vatNumber, DateTimeOffset requestDate, string name, string address, bool isValid) : this()
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