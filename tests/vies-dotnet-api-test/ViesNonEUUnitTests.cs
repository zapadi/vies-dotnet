/*
   Copyright 2017-2023 Adrian Popescu.
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

using Xunit;

namespace Padi.Vies.Test;

public sealed class ViesNonEUUnitTests
{
    [Theory]
    [InlineData("GB434031494", true)]
    [InlineData("GBGD001", true)]
    [InlineData("GBHA500", true)]
    [InlineData("GB434031493", false)]
    [InlineData("GB12345", false)]
    [InlineData("GBGD500", false)]
    [InlineData("GBHA100", false)]
    [InlineData("GB12345678", false)]
    public void Should_Validate_GB_Vat(string vatNumber, bool isValid)
    {
        var result = ViesManager.IsValid(vatNumber);

        Assert.Equal("Great Britain(GB) is no longer supported by VIES services provided by EC since 2021-01-01 because of Brexit", result.Error);
    }

    [Theory]
    [InlineData("925901618", true)]
    [InlineData("GD001", true)]
    [InlineData("HA500", true)]
    [InlineData("434031493", false)]
    [InlineData("12345", false)]
    [InlineData("GD500", false)]
    [InlineData("HA100", false)]
    [InlineData("12345678", false)]
    public void Should_Validate_XI_Vat(string vatNumber, bool isValid)
    {
        var result = ViesManager.IsValid("XI", vatNumber);
        Assert.Equal(isValid, result.IsValid);
    }
}
