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
namespace Padi.Vies.Errors;

internal static class VatValidationDispatcher
{
    public static VatValidationResult InvalidVatFormat(string countryCode, string vatNumber, string userMessage = null)
    {
        return VatValidationResult.Failed(
            countryCode: countryCode,
            errorCode: ViesErrorCodes.ValidationError.InvalidVatFormat.Code,
            errorMessage: $"{ViesErrorCodes.ValidationError.InvalidVatFormat.Message}: {vatNumber}.",
            param: ViesErrorCodes.ValidationError.InvalidVatFormat.Param,
            userMessage: userMessage ?? ViesErrorCodes.ValidationError.InvalidVatFormat.UserMessage
        );
    }

    public static VatValidationResult InvalidVatChecksumDigit(string countryCode, string vatNumber, string userMessage = null)
    {
        return VatValidationResult.Failed(
            countryCode: countryCode,
            errorCode: ViesErrorCodes.ValidationError.InvalidVatChecksumDigit.Code,
            errorMessage: $"{ViesErrorCodes.ValidationError.InvalidVatChecksumDigit.Message}: {vatNumber}.",
            userMessage: userMessage ?? ViesErrorCodes.ValidationError.InvalidVatChecksumDigit.UserMessage
        );
    }

    public static VatValidationResult InvalidCountryCode(string countryCode, string userMessage = null)
    {
        return VatValidationResult.Failed(
            countryCode: countryCode,
            errorCode: ViesErrorCodes.ValidationError.InvalidCountryCode.Code,
            errorMessage: $"{ViesErrorCodes.ValidationError.InvalidCountryCode.Message}: {countryCode}.",
            param: ViesErrorCodes.ValidationError.InvalidCountryCode.Param,
            userMessage: userMessage ?? ViesErrorCodes.ValidationError.InvalidCountryCode.UserMessage
        );
    }

    public static VatValidationResult VatNumberTooLong(string countryCode,string vatNumber)
    {
        return VatValidationResult.Failed(
            countryCode: countryCode,
            errorCode: ViesErrorCodes.ValidationError.VatNumberTooLong.Code,
            errorMessage: $"{ViesErrorCodes.ValidationError.VatNumberTooLong.Message}: {vatNumber}.",
            param: ViesErrorCodes.ValidationError.VatNumberTooLong.Param,
            userMessage: ViesErrorCodes.ValidationError.VatNumberTooLong.UserMessage
        );
    }

    public static VatValidationResult RegionUnsupported(string countryCode, string userMessage = null)
    {
        throw new ViesUnsupportedRegionException(
            errorCode: ViesErrorCodes.UnsupportedRegionError.RegionUnsupported.Code,
            message: $"{ViesErrorCodes.UnsupportedRegionError.RegionUnsupported.Message} (Country: {countryCode}).",
            param: ViesErrorCodes.UnsupportedRegionError.RegionUnsupported.Param,
            userMessage: userMessage
        );
    }
}
