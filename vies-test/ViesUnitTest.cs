/*
   Copyright 2017 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the spevatic language governing permissions and
   limitations under the License.
*/

using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Zapadi.Vies.Test
{

    [Collection("ViesCollection")]
    public class ViesUnitTest
    {
        private readonly ViesManagerFixture fixture;
        public ViesUnitTest(ViesManagerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ShouldReturnVatIsActive()
        {
            var a = await fixture.ViesManager.CheckIfActive("LU26375245", CancellationToken.None).ConfigureAwait(false);

            Assert.True(a.IsValid, "Inactive vat number");
            Assert.True(a.CountryCode == "LU", "Wrong country code.");
            Assert.True(a.VATNumber == "26375245", "Wrong vat number.");
        }

         [Theory]
         [InlineData("AT U12345678","ATU12345678")]
         [InlineData("  ATU12345678","ATU12345678")]
         [InlineData("AT - -U12345678","ATU12345678")]
         [InlineData("at-U-12345678 ","ATU12345678")]
         
        public void TestSanitizeVatNumber(string inputVatNumber, string expectedVatNumber)
        {
            Assert.True(VatValidation.Sanitize(inputVatNumber).Equals(expectedVatNumber), "Wrong sanitized vat number.");
        }

        [Theory]
        [InlineData("BE123456789")]
        [InlineData("IE 123")]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("ATU1234567")]
        [InlineData("BE012345678")]
        [InlineData("BE123456789")]
        [InlineData("BG1234567")]
        [InlineData("CY1234567X")]
        [InlineData("CZ1234567")]
        [InlineData("DK10000030")]
        [InlineData("DK1234567")]
        [InlineData("EE12345678")]
        [InlineData("EL12345678")]
        [InlineData("ESX1234567")]
        [InlineData("FI1234567")]
        [InlineData("FR1234567890")]
        [InlineData("GB99999997")]
        [InlineData("HU1234567")]
        [InlineData("HR1234567890")]
        [InlineData("IE123456X")]
        [InlineData("IT1234567890")]
        [InlineData("LT12345678")]
        [InlineData("LU1234567")]
        [InlineData("LU10000350")]
        [InlineData("LV1234567890")]
        [InlineData("MT1234567")]
        [InlineData("NL12345678B12")]
        [InlineData("PL123456789")]
        [InlineData("PT12345678")]
        [InlineData("RO1")]
        [InlineData("RO11198698")]
        [InlineData("RO99907")]
        [InlineData("RO18")]
        [InlineData("RO00099908")]
        [InlineData("RO0000099908")]
        [InlineData("SE12345678901")]
        [InlineData("SE556188840301")]
        [InlineData("SE000000003301")]
        [InlineData("SI1234567")]
        [InlineData("SI05936241")]
        [InlineData("SK5407062531")]
        [InlineData("SK7020001680")]
        [InlineData("AB123A01")]
        [InlineData("ATU1234567")]
        [InlineData("BE012345678")]
        [InlineData("NL12345678B12")]        
        public void  ShouldThrowViesValidationException(string vatNumber)
        {
             Assert.Throws<ViesValidationException>(() =>
                 VatValidation.ParseVat(vatNumber)
             );
        }

        [Theory]
        [InlineData("LU26375245")]
        [InlineData("PL1234567890")]
        [InlineData("PT123456789")]
        [InlineData("RO123456789")]
        public void ShouldValidate(string vatNumber)
        {
            var result = VatValidation.ParseVat(vatNumber);
        }
    }
}