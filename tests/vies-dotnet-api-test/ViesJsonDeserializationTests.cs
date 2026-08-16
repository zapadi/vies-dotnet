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
using System.Text;
using Padi.Vies.Errors;
using Padi.Vies.Parsers;
using Padi.Vies.Parsers.Json;
using Xunit;

namespace Padi.Vies.Test;

public sealed class ViesJsonDeserializationTests
{
    private readonly IResponseParser _parser = new JsonResponseParser();

    private const string ValidJson = TestPayloads.ValidJson;

    private const string InvalidVatJson =
        """
        {
          "countryCode": "RO", "vatNumber": "123456789",
          "requestDate": "2026-07-16T10:23:03.183Z",
          "valid": false, "requestIdentifier": "", "name": "", "address": "",
          "traderName": "---", "traderStreet": "---"
        }
        """;

    private const string ErrorJson = TestPayloads.ErrorJson;

    private ViesCheckVatResponse Parse(string json) => _parser.Parse(Encoding.UTF8.GetBytes(json));

    [Fact]
    public void Should_Deserialize_Valid()
    {
        ViesCheckVatResponse response = Parse(ValidJson);

        Assert.Equal("RO", response.CountryCode);
        Assert.Equal("123456789", response.VatNumber);
        Assert.True(response.IsValid);
        Assert.Equal("ACME SRL", response.Name);
        Assert.Equal("STR. EXAMPLE 1", response.Address);
        Assert.Equal("WAPIAAAAY123", response.RequestIdentifier);
        Assert.Equal("ACME SRL", response.TraderName);
        Assert.Equal("BUCURESTI", response.TraderCity);
        Assert.Equal(2026, response.RequestDate.Year);
    }

    [Fact]
    public void Should_Deserialize_Valid_With_Utf8_Bom()
    {
        // Utf8JsonReader (unlike JsonSerializer/JsonDocument) does not skip a leading UTF-8 BOM,
        // so the parser strips it itself; this pins that behaviour.
        byte[] preamble = Encoding.UTF8.GetPreamble();
        byte[] body = Encoding.UTF8.GetBytes(ValidJson);
        var withBom = new byte[preamble.Length + body.Length];
        preamble.CopyTo(withBom, 0);
        body.CopyTo(withBom, preamble.Length);

        ViesCheckVatResponse response = _parser.Parse(withBom);

        Assert.True(response.IsValid);
        Assert.Equal("RO", response.CountryCode);
    }

    [Fact]
    public void Should_Deserialize_Invalid_Vat()
    {
        ViesCheckVatResponse response = Parse(InvalidVatJson);

        Assert.False(response.IsValid);
    }

    [Fact]
    public void Should_Throw_ViesServiceException_On_ErrorWrappers()
    {
        ViesServiceException exception = Assert.Throws<ViesServiceException>(() => Parse(ErrorJson));

        Assert.Equal(ViesErrorCodes.InputError.InvalidInput.Code, exception.ErrorCode);
    }

    [Fact]
    public void Should_Throw_ViesDeserializationException_On_Garbage()
    {
        Assert.Throws<ViesDeserializationException>(() => Parse("this is not json"));
    }

    [Fact]
    public void Should_Throw_ViesDeserializationException_On_Empty_Object()
    {
        Assert.Throws<ViesDeserializationException>(() => Parse("{}"));
    }

    [Fact]
    public void Should_Not_Throw_Nre_On_Null_ErrorWrapper_Element()
    {
        const string json = """{"actionSucceed":false,"errorWrappers":[null]}""";

        ViesServiceException exception = Assert.Throws<ViesServiceException>(() => Parse(json));

        Assert.Equal(ViesErrorCodes.ServiceError.InvalidResponse.Code, exception.ErrorCode);
    }

    [Fact]
    public void Should_Use_Message_When_Error_Is_Null()
    {
        const string json = """{"actionSucceed":false,"errorWrappers":[{"message":"Blocked"}]}""";

        ViesServiceException exception = Assert.Throws<ViesServiceException>(() => Parse(json));

        Assert.Contains("Blocked", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_Map_ActionSucceed_False_With_Empty_Wrappers_To_InvalidResponse()
    {
        const string json = """{"actionSucceed":false,"errorWrappers":[]}""";

        ViesServiceException exception = Assert.Throws<ViesServiceException>(() => Parse(json));

        Assert.Equal(ViesErrorCodes.ServiceError.InvalidResponse.Code, exception.ErrorCode);
    }

    [Fact]
    public void Should_Map_RateLimit_Wrapper()
    {
        const string json = """{"actionSucceed":false,"errorWrappers":[{"error":"MS_MAX_CONCURRENT_REQ"}]}""";

        ViesServiceException exception = Assert.Throws<ViesServiceException>(() => Parse(json));

        Assert.Equal(ViesErrorCodes.ServiceError.RateLimitExceeded.Code, exception.ErrorCode);
    }
}
