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
    public void Should_Return_Correct_Error_Code_For_AT_Vat(string vat, string expectedErrorCode)
    {
        // Act
        VatValidationResult result = ViesManager.IsValid(vat);

        // Assert
        AssertValidationResult(result, "AT", expectedErrorCode);
    }

    // [Theory]
    // [InlineData("BE12345678", VatValidationErrorCode.InvalidLength)] // Too short (8 chars)
    // [InlineData("BE12345678901", VatValidationErrorCode.InvalidLength)] // Too long (11 chars)
    // [InlineData("BE2123456789", VatValidationErrorCode.InvalidFormat)] // 10-digit number starting with 2
    // [InlineData("BE12345678X", VatValidationErrorCode.InvalidFormat)] // Contains non-digit
    // [InlineData("BE123456789", VatValidationErrorCode.InvalidCheckDigit)] // Invalid checksum for 9-digit
    // [InlineData("BE012345678", VatValidationErrorCode.InvalidCheckDigit)] // Invalid checksum for 10-digit
    // public void Should_Return_Correct_Error_Code_For_BE_Vat(string vat, string expectedErrorCode)
    // {
    //     // Act
    //     VatValidationResult result = ViesManager.IsValid(vat);
    //
    //     // Assert
    //     AssertValidationResult(result, "BE", expectedErrorCode);
    // }
    //
    // [Theory]
    // [InlineData("BG12345678", VatValidationErrorCode.InvalidLength)] // Too short (8 chars)
    // [InlineData("BG12345678901", VatValidationErrorCode.InvalidLength)] // Too long (11 chars)
    // [InlineData("BG12345678X", VatValidationErrorCode.InvalidFormat)] // Contains non-digit
    // [InlineData("BG123456789", VatValidationErrorCode.InvalidCheckDigit)] // Invalid checksum for 9-digit
    // [InlineData("BG1234567890", VatValidationErrorCode.InvalidFormat)] // Invalid date format for 10-digit
    // public void Should_Return_Correct_Error_Code_For_BG_Vat(string vat, string expectedErrorCode)
    // {
    //     // Act
    //     VatValidationResult result = ViesManager.IsValid(vat);
    //
    //     // Assert
    //     AssertValidationResult(result, "BG", expectedErrorCode);
    // }
    //
    // [Theory]
    // [InlineData("CY1234567X", VatValidationErrorCode.InvalidLength)] // Too short (8 chars)
    // [InlineData("CY1234567890X", VatValidationErrorCode.InvalidLength)] // Too long (11 chars)
    // [InlineData("CY12345678X", VatValidationErrorCode.InvalidFormat)] // First 8 chars must be digits
    // [InlineData("CY123456789", VatValidationErrorCode.InvalidFormat)] // 9th char must be letter
    // [InlineData("CY121234567X", VatValidationErrorCode.InvalidFormat)] // Cannot start with 12
    // [InlineData("CY10000314K", VatValidationErrorCode.InvalidCheckDigit)] // Invalid checksum
    // public void Should_Return_Correct_Error_Code_For_CY_Vat(string vat, string expectedErrorCode)
    // {
    //     // Act
    //     VatValidationResult result = ViesManager.IsValid(vat);
    //
    //     // Assert
    //     AssertValidationResult(result, "CY", expectedErrorCode);
    // }
    //
    // [Theory]
    // [InlineData("CZ1234567", VatValidationErrorCode.InvalidLength)] // Too short (7 chars)
    // [InlineData("CZ12345678901234", VatValidationErrorCode.InvalidLength)] // Too long (14 chars)
    // [InlineData("CZ1234567X", VatValidationErrorCode.InvalidFormat)] // Contains non-digit
    // [InlineData("CZ12345678", VatValidationErrorCode.InvalidCheckDigit)] // Invalid checksum for 8-digit
    // public void Should_Return_Correct_Error_Code_For_CZ_Vat(string vat, string expectedErrorCode)
    // {
    //     // Act
    //     VatValidationResult result = ViesManager.IsValid(vat);
    //
    //     // Assert
    //     AssertValidationResult(result, "CZ", expectedErrorCode);
    // }
    //
    // [Theory]
    // [InlineData("DE12345678", VatValidationErrorCode.InvalidLength)] // Too short (8 chars)
    // [InlineData("DE1234567890", VatValidationErrorCode.InvalidLength)] // Too long (10 chars)
    // [InlineData("DE023456789", VatValidationErrorCode.InvalidFormat)] // First digit must be 1-9
    // [InlineData("DE12345678X", VatValidationErrorCode.InvalidFormat)] // Contains non-digit
    // [InlineData("DE123456789", VatValidationErrorCode.InvalidCheckDigit)] // Invalid checksum
    // public void Should_Return_Correct_Error_Code_For_DE_Vat(string vat, string expectedErrorCode)
    // {
    //     // Act
    //     VatValidationResult result = ViesManager.IsValid(vat);
    //
    //     // Assert
    //     AssertValidationResult(result, "DE", expectedErrorCode);
    // }
    //
    // [Theory]
    // [InlineData("DK12345678901234", VatValidationErrorCode.InvalidLength)] // Too long (>12 digits)
    // [InlineData("DK1234567", VatValidationErrorCode.InvalidFormat)] // Too short (7 digits)
    // [InlineData("DK1234567X", VatValidationErrorCode.InvalidFormat)] // Contains non-digit
    // [InlineData("DK26134438", VatValidationErrorCode.InvalidCheckDigit)] // Invalid checksum
    // public void Should_Return_Correct_Error_Code_For_DK_Vat(string vat, string expectedErrorCode)
    // {
    //     // Act
    //     VatValidationResult result = ViesManager.IsValid(vat);
    //
    //     // Assert
    //     AssertValidationResult(result, "DK", expectedErrorCode);
    // }

    private static void AssertValidationResult(VatValidationResult result, string expectedCountryCode, string expectedErrorCode)
    {
        Assert.False(result.IsValid);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(expectedCountryCode, result.CountryCode);
    }
}
