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

using System.Text.Json.Serialization;

namespace Padi.Vies.Parsers.Json;

internal sealed class CheckVatRestRequest
{
    [JsonPropertyName("countryCode")] public string CountryCode { get; set; }
    [JsonPropertyName("vatNumber")] public string VatNumber { get; set; }
}

internal sealed class CheckVatRestResult
{
    [JsonPropertyName("countryCode")] public string CountryCode { get; set; }
    [JsonPropertyName("vatNumber")] public string VatNumber { get; set; }
    [JsonPropertyName("requestDate")] public string RequestDate { get; set; }
    [JsonPropertyName("valid")] public bool? Valid { get; set; }
    [JsonPropertyName("requestIdentifier")] public string RequestIdentifier { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("address")] public string Address { get; set; }
    [JsonPropertyName("traderName")] public string TraderName { get; set; }
    [JsonPropertyName("traderStreet")] public string TraderStreet { get; set; }
    [JsonPropertyName("traderPostalCode")] public string TraderPostalCode { get; set; }
    [JsonPropertyName("traderCity")] public string TraderCity { get; set; }
    [JsonPropertyName("traderCompanyType")] public string TraderCompanyType { get; set; }
    [JsonPropertyName("actionSucceed")] public bool? ActionSucceed { get; set; }
    [JsonPropertyName("errorWrappers")] public RestErrorWrapper[] ErrorWrappers { get; set; }
}

internal sealed class RestErrorWrapper
{
    [JsonPropertyName("error")] public string Error { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; }
}
