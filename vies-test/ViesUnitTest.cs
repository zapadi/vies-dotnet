using System.Threading;
using Zapadi.Vies;
using Xunit;
using System.Threading.Tasks;
using System;

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
        public async Task ShouldReturnValidResult()
        {
            var a = await fixture.ViesManager.CheckVat("LU26375245", CancellationToken.None).ConfigureAwait(false);

            Assert.True(a.IsValid, "Invalid vat number");
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
            Assert.True(inputVatNumber.SanitizeVatNumber().Equals(expectedVatNumber), "Wrong sanitized vat number.");
        }

        [Theory]
        [InlineData("BE123456789", false)]
        [InlineData("IE 123", false)]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("ATU1234567", false)]
        [InlineData("BE012345678", false)]
        [InlineData("BE123456789", false)]
        [InlineData("BG1234567", false)]
        [InlineData("CY1234567X", false)]
        [InlineData("CZ1234567", false)]
        [InlineData("DE12345678", false)]
        [InlineData("DK1234567", false)]
        [InlineData("EE12345678", false)]
        [InlineData("EL12345678", false)]
        [InlineData("ESX1234567", false)]
        [InlineData("FI1234567", false)]
        [InlineData("FR1234567890", false)]
        [InlineData("GB99999997", false)]
        [InlineData("HU1234567", false)]
        [InlineData("HR1234567890", false)]
        [InlineData("IE123456X", false)]
        [InlineData("IT1234567890", false)]
        [InlineData("LT12345678", false)]
        [InlineData("LU1234567", false)]
        [InlineData("LV1234567890", false)]
        [InlineData("MT1234567", false)]
        [InlineData("NL12345678B12", false)]
        [InlineData("PL123456789", false)]
        [InlineData("PT12345678", false)]
        [InlineData("RO1", false)] // Romania has a really weird VAT format...
        [InlineData("SE12345678901", false)]
        [InlineData("SI1234567", false)]
        [InlineData("SK123456789", false)]
        [InlineData("AB123A01", false)]
        [InlineData("ATU1234567", false)]
        [InlineData("BE012345678", false)]
        [InlineData("NL12345678B12", false)]
        public async Task ShouldThrowValidateException(string vatNumber, bool notUsed)
        {
            await Assert.ThrowsAsync<ValidationException>(async () =>
                 await fixture.ViesManager.CheckVat(vatNumber, CancellationToken.None).ConfigureAwait(false)
             ).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("LU26375245")]
        [InlineData("NL123456789B01")]
        [InlineData("ATU12345678")]
        [InlineData("BE0123456789")]
        [InlineData("BE1234567891")]
        [InlineData("BG123456789")]
        [InlineData("BG1234567890")]
        [InlineData("CY12345678X")]
        [InlineData("CZ12345678")]
        [InlineData("DE123456789")]
        [InlineData("DK12345678")]
        [InlineData("EE123456789")]
        [InlineData("EL123456789")]
        [InlineData("ESX12345678")]
        [InlineData("FI12345678")]
        [InlineData("FR12345678901")]
        [InlineData("GB999999973")]
        [InlineData("HU12345678")]
        [InlineData("HR12345678901")]
        [InlineData("IE1234567X")]
        [InlineData("IT12345678901")]
        [InlineData("LT123456789")]
        [InlineData("LU12345678")]
        [InlineData("LV12345678901")]
        [InlineData("MT12345678")]
        [InlineData("NL123456789B12")]
        [InlineData("PL1234567890")]
        [InlineData("PT123456789")]
        [InlineData("RO123456789")]
        [InlineData("SE123456789012")]
        [InlineData("SI12345678")]
        [InlineData("SK1234567890")]
        public async Task ShouldValidate(string vatNumber)
        {
            var result = await fixture.ViesManager.CheckVat(vatNumber, CancellationToken.None).ConfigureAwait(false);

            Assert.True(result.IsValid, $"{vatNumber} is invalid.");
        }
    }
}