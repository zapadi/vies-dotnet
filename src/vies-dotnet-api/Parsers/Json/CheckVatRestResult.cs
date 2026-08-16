/*
   Copyright 2017-2026 Adrian Popescu.
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

namespace Padi.Vies.Parsers.Json;

internal sealed class CheckVatRestResult
{
    public string? CountryCode { get; set; }
    public string? VatNumber { get; set; }
    public string? RequestDate { get; set; }
    public bool? Valid { get; set; }
    public string? RequestIdentifier { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? TraderName { get; set; }
    public string? TraderStreet { get; set; }
    public string? TraderPostalCode { get; set; }
    public string? TraderCity { get; set; }
    public string? TraderCompanyType { get; set; }
    public bool? ActionSucceed { get; set; }
    public RestErrorWrapper?[]? ErrorWrappers { get; set; }
}
