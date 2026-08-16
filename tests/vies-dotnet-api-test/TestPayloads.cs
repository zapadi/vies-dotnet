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

namespace Padi.Vies.Test;

internal static class TestPayloads
{
    public const string ValidJson =
        """
        {
          "countryCode": "RO", "vatNumber": "123456789",
          "requestDate": "2026-07-16T10:23:03.183Z",
          "valid": true, "requestIdentifier": "WAPIAAAAY123",
          "name": "ACME SRL", "address": "STR. EXAMPLE 1",
          "traderName": "ACME SRL", "traderStreet": "STR. EXAMPLE 1",
          "traderPostalCode": "010101", "traderCity": "BUCURESTI",
          "traderCompanyType": "SRL",
          "traderNameMatch": "VALID", "traderStreetMatch": "VALID",
          "traderPostalCodeMatch": "VALID", "traderCityMatch": "VALID",
          "traderCompanyTypeMatch": "VALID"
        }
        """;

    public const string ErrorJson = """{"actionSucceed":false,"errorWrappers":[{"error":"INVALID_INPUT"}]}""";
}
