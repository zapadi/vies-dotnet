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

/// <summary>
///
/// </summary>
#pragma warning disable CA1032
public class ViesException : Exception
#pragma warning restore CA1032
{
    /// <summary>
    ///
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    ///
    /// </summary>
    public string ErrorType { get; }

    /// <summary>
    ///
    /// </summary>
    public string Param { get; }

    /// <summary>
    ///
    /// </summary>
    public string UserMessage { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="errorType"></param>
    /// <param name="message"></param>
    /// <param name="param"></param>
    /// <param name="userMessage"></param>
    public ViesException(string errorCode, string errorType, string message, string param = null, string userMessage = null)
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorType = errorType;
        Param = param;
        UserMessage = userMessage ?? message;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="errorType"></param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    /// <param name="param"></param>
    /// <param name="userMessage"></param>
    public ViesException(string errorCode, string errorType, string message, Exception innerException, string param = null, string userMessage = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        ErrorType = errorType;
        Param = param;
        UserMessage = userMessage ?? message;
    }
}
