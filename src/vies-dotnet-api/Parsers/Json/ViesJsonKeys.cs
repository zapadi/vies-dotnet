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

using System;

namespace Padi.Vies.Parsers.Json;

internal static class ViesJsonKeys
{
    public static ReadOnlySpan<byte> CountryCode => "countryCode"u8;
    public static ReadOnlySpan<byte> VatNumber => "vatNumber"u8;
    public static ReadOnlySpan<byte> RequestDate => "requestDate"u8;
    public static ReadOnlySpan<byte> Valid => "valid"u8;
    public static ReadOnlySpan<byte> RequestIdentifier => "requestIdentifier"u8;
    public static ReadOnlySpan<byte> Name => "name"u8;
    public static ReadOnlySpan<byte> Address => "address"u8;
    public static ReadOnlySpan<byte> TraderName => "traderName"u8;
    public static ReadOnlySpan<byte> TraderStreet => "traderStreet"u8;
    public static ReadOnlySpan<byte> TraderPostalCode => "traderPostalCode"u8;
    public static ReadOnlySpan<byte> TraderCity => "traderCity"u8;
    public static ReadOnlySpan<byte> TraderCompanyType => "traderCompanyType"u8;
    public static ReadOnlySpan<byte> ActionSucceed => "actionSucceed"u8;
    public static ReadOnlySpan<byte> ErrorWrappers => "errorWrappers"u8;
    public static ReadOnlySpan<byte> Error => "error"u8;
    public static ReadOnlySpan<byte> Message => "message"u8;
}
