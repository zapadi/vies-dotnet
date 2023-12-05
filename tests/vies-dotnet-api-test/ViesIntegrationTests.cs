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

using System.Threading.Tasks;
using Padi.Vies.Errors;
using Xunit;

namespace Padi.Vies.Test;

[Collection("ViesCollection")]
public sealed class ViesIntegrationTests
{
    private readonly ViesManagerFixture _fixture;

    public ViesIntegrationTests(ViesManagerFixture fixture)
    {
        this._fixture = fixture;
    }

    [Theory]
    [InlineData("LU26375245")]
    [InlineData("SE 556656688001")]
    [InlineData("FI 09073468")]
    [InlineData("NL 858292828B01")]
    [InlineData("FR 66322120916")]
    [InlineData("IT 01640320360")]
    [InlineData("RO26129093")]
    [InlineData("SK2120046819")]
    public async Task Should_Return_Vat_Active(string vat)
    {
        var actual = await this.CheckIfActiveAsync(vat, true);

        Assert.True(actual.IsValid, "Inactive vat number");
    }
        
    [Theory]
    [InlineData("RO123456789")]
    [InlineData("ATU12345675")]
    [InlineData("CZ612345670")]
    [InlineData("ESK1234567L")]
    [InlineData("IE1234567T")]
    [InlineData("NL123456782B90")]
    public async Task Should_Return_Vat_Inactive(string vat)
    {
        var actual = await this.CheckIfActiveAsync(vat, false);

        Assert.False(actual.IsValid, "Inactive vat number");
    }

    [Theory]
    [InlineData("GB434031494")]
    [InlineData("123")]
    [InlineData("K99999999L")] //Albania
    [InlineData("CHE-123.456.788 ")]
    public async Task Should_Throw_ViesServiceException(string vat)
    {
        await Assert.ThrowsAsync<ViesServiceException>(() => this.CheckIfActiveAsync(vat, false));
    }
        
    private async Task<ViesCheckVatResponse> CheckIfActiveAsync(string vat, bool mockValue){
            
        var actual = await this._fixture.ViesManager.IsActiveAsync(vat);
            
        return actual;
    }
}