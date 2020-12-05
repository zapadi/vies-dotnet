/*
   Copyright 2017-2019 Adrian Popescu.
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

namespace Padi.Vies
{
    public struct ViesCheckVatResponse
    {
        public ViesCheckVatResponse(string countryCode, string vatNumber, DateTimeOffset requestDate, string name = null, string address = null, bool isValid = false) : this()
        {
            CountryCode = countryCode;
            VatNumber = vatNumber;
            RequestDate = requestDate;
            Name = name;
            Address = address;
            IsValid = isValid;
        }

        public string CountryCode { get; }
        public string VatNumber { get; }
        public DateTimeOffset RequestDate { get; }
        public string Name { get; }
        public string Address { get; }
        public bool IsValid { get; }
    }
}