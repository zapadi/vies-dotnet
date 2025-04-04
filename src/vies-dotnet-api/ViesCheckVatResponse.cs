/*
   Copyright 2017-2025 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;

namespace Padi.Vies;

public record struct ViesCheckVatResponse(
    string CountryCode,
    string VatNumber,
    DateTimeOffset RequestDate,
    string Name = null,
    string Address = null,
    bool IsValid = false)
{
    public string CountryCode { get; internal set; } = CountryCode;
    public string VatNumber { get; internal set; } = VatNumber;
    public DateTimeOffset RequestDate { get; internal set;} = RequestDate;
    public string Name { get; internal set;} = Name;
    public string Address { get; internal set;} = Address;
    public bool IsValid { get; internal set;} = IsValid;
}
