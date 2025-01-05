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

using System.Threading.Tasks;
using Padi.Vies.Errors;
using Xunit;

namespace Padi.Vies.Test;

[Collection("ViesCollection")]
public sealed class ViesIntegrationTests(ViesManagerFixture fixture)
{
    [Theory]
    [InlineData("IE8D79739I")]
    [InlineData("IE8Y41127O")]
    [InlineData("NL002101624B69")]
    public async Task Should_Return_Vat_Active(string vat)
    {
        ViesCheckVatResponse actual = await CheckIfActiveAsync(vat);

        Assert.True(actual.IsValid, $"Inactive {actual.CountryCode} vat number");
    }

    [Theory]
    [InlineData("ATU12345675")]
    [InlineData("CY10014000M")]
    [InlineData("CZ612345670")]
    [InlineData("ESK1234567L")]
    [InlineData("IE1234567T")]
    [InlineData("IE6433435OA")]
    [InlineData("NL123456782B90")]
    [InlineData("NL123456789B13")]
    [InlineData("RO123456789")]
    public async Task Should_Return_Vat_Inactive(string vat)
    {
        ViesCheckVatResponse actual = await CheckIfActiveAsync(vat);

        Assert.False(actual.IsValid, $"Inactive {actual.CountryCode} vat number");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("CHE-123.456.788 ")]
    [InlineData("GB434031494")]
    [InlineData("K99999999L")] //Albania
    public async Task Should_Throw_ViesServiceException(string vat)
    {
        await Assert.ThrowsAsync<ViesServiceException>(() => CheckIfActiveAsync(vat));
    }

    private async Task<ViesCheckVatResponse> CheckIfActiveAsync(string vat, bool mockValue = false){

        ViesCheckVatResponse actual = await fixture.ViesManager.IsActiveAsync(vat);

        return actual;
    }
}
