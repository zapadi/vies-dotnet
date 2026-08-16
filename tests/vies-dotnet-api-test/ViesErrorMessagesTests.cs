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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Padi.Vies.Errors;
using Xunit;

namespace Padi.Vies.Test;

public sealed class ViesErrorMessagesTests
{
    private static IEnumerable<string> GetAllErrorCodes()
    {
        return typeof(ViesErrorCode)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(static field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(static field => (string)field.GetRawConstantValue()!);
    }

    public static TheoryData<string> ErrorCodes()
    {
        var data = new TheoryData<string>();
        foreach (var code in GetAllErrorCodes())
        {
            data.Add(code);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ErrorCodes))]
    public void Should_Return_Default_Message_For_Known_Code(string errorCode)
    {
        var message = ViesErrorMessages.GetDefaultMessage(errorCode);

        Assert.False(string.IsNullOrEmpty(message));
    }

    [Theory]
    [MemberData(nameof(ErrorCodes))]
    public void Should_Return_Default_User_Message_For_Known_Code(string errorCode)
    {
        var userMessage = ViesErrorMessages.GetDefaultUserMessage(errorCode);

        Assert.False(string.IsNullOrEmpty(userMessage));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("no-such-error-code")]
    public void Should_Return_Null_For_Unknown_Code(string? errorCode)
    {
        Assert.Null(ViesErrorMessages.GetDefaultMessage(errorCode));
        Assert.Null(ViesErrorMessages.GetDefaultUserMessage(errorCode));
    }

    [Fact]
    public void Should_Have_Unique_Error_Codes()
    {
        var codes = GetAllErrorCodes().ToList();

        Assert.NotEmpty(codes);
        Assert.Equal(codes.Count, codes.Distinct(System.StringComparer.Ordinal).Count());
    }
}
