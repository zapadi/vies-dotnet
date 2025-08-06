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

using System;

namespace Padi.Vies.Errors;

public class ViesException : Exception
{
    public string ErrorCode { get; }
    public string ErrorType { get; }
    public string Param { get; }
    public string UserMessage { get; }

    public ViesException(string errorCode, string errorType, string message, string param = null, string userMessage = null)
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorType = errorType;
        Param = param;
        UserMessage = userMessage ?? message;
    }

    public ViesException(string errorCode, string errorType, string message, Exception innerException, string param = null, string userMessage = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        ErrorType = errorType;
        Param = param;
        UserMessage = userMessage ?? message;
    }
}
