/*
   Copyright 2017-2024 Adrian Popescu.
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

using Padi.Vies.Errors;
using Xunit;

namespace Padi.Vies.Test;

public class VatValidationErrorTests
{
    [Theory]
    [InlineData("ATU1234567", ViesErrorCodes.ValidationError.InvalidVatFormat.Code)] // Too short (8 chars)
    [InlineData("ATU123456789", ViesErrorCodes.ValidationError.InvalidVatFormat.Code)] // Too long (10 chars)
    [InlineData("ATA12345678", ViesErrorCodes.ValidationError.InvalidVatFormat.Code)] // Wrong first char (A instead of U)
    [InlineData("ATU1234567X", ViesErrorCodes.ValidationError.InvalidVatFormat.Code)] // Non-digit in position 8
    [InlineData("ATUX2345678", ViesErrorCodes.ValidationError.InvalidVatFormat.Code)] // Non-digit in position 2
    [InlineData("ATU12345677", ViesErrorCodes.ValidationError.InvalidVatChecksumDigit.Code)] // Invalid checksum
    // ReSharper disable once InconsistentNaming
    public void Should_Return_Correct_ErrorCode_AT_VAT(string vat, string expectedErrorCode)
    {
        // Act
        VatValidationResult result = ViesManager.IsValid(vat);

        // Assert
        AssertValidationResult(result, "AT", expectedErrorCode);
    }

    private static void AssertValidationResult(VatValidationResult result, string expectedCountryCode, string expectedErrorCode)
    {
        Assert.False(result.IsValid);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(expectedCountryCode, result.CountryCode);
    }
}
