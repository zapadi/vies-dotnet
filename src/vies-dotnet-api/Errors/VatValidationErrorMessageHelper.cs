using System.Globalization;
using System.Runtime.CompilerServices;

namespace Padi.Vies.Errors;

/// <summary>
/// Helper to create validation error messages
/// </summary>
internal static class VatValidationErrorMessageHelper
{
    private const string InvalidNumber = "Number is invalid";
    private const string InvalidFormat = "Invalid format";
    private const string InvalidFormatDigits = "All characters must be digits";
    private const string InvalidChecksum = "Invalid check digit";

    /// <summary>
    /// Creates a validation error for invalid length
    /// </summary>
    /// <param name="length">The required length</param>
    /// <returns>The error message</returns>
    public static string GetLengthMessage(uint length) =>
        $"Required length: {length.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>
    /// Creates a validation error for invalid number
    /// </summary>
    /// <returns>The error message</returns>
    public static string GetInvalidNumberMessage() =>
        InvalidNumber;

    /// <summary>
    /// Creates a validation error for invalid length range
    /// </summary>
    /// <param name="start">The minimum length</param>
    /// <param name="end">The maximum length</param>
    /// <returns>The error message</returns>
    public static string GetLengthRangeMessage(uint start, uint end) =>
        $"Required length: {start.ToString(CultureInfo.InvariantCulture)} to {end.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>
    /// Creates a validation error for invalid first digit range
    /// </summary>
    /// <param name="start">The minimum digit</param>
    /// <param name="end">The maximum digit</param>
    /// <returns>The error message</returns>
    public static string GetInvalidFirstDigitRangeMessage(uint start, uint end) =>
        $"First character must be a digit between {start.ToString(CultureInfo.InvariantCulture)} and {end.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>
    /// Creates a validation error for invalid character at position
    /// </summary>
    /// <param name="position">The position</param>
    /// <param name="value">The expected value</param>
    /// <returns>The error message</returns>
    public static string GetInvalidCharacterAtMessage(uint position, string value) =>
        $"Character at position {position.ToString(CultureInfo.InvariantCulture)} must be {value}";

    /// <summary>
    /// Creates a validation error for invalid prefix
    /// </summary>
    /// <param name="start">The prefix length</param>
    /// <param name="text">The expected prefix</param>
    /// <returns>The error message</returns>
    public static string GetInvalidPrefixMessage(uint start, string text) =>
        $"First {start.ToString(CultureInfo.InvariantCulture)} characters must be '{text}'";

    /// <summary>
    /// Creates a validation error for invalid range digits
    /// </summary>
    /// <param name="start">The start position</param>
    /// <param name="end">The end position</param>
    /// <returns>The error message</returns>
    public static string GetInvalidRangeDigitsMessage(uint start, uint end) =>
        $"Characters between {start.ToString(CultureInfo.InvariantCulture)} and {end.ToString(CultureInfo.InvariantCulture)} must be digits";

    /// <summary>
    /// Creates a validation error for invalid length exceed
    /// </summary>
    /// <param name="length">The maximum length</param>
    /// <returns>The error message</returns>
    public static string GetLengthExceedMessage(uint length) =>
        $"Length exceeds {length.ToString(CultureInfo.InvariantCulture)} characters";

    /// <summary>
    /// Creates a validation error for all digits
    /// </summary>
    /// <returns>The error message</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetAllDigitsMessage() =>
        InvalidFormatDigits;

    /// <summary>
    /// Creates a validation error for invalid format
    /// </summary>
    /// <returns>The error message</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetInvalidFormatMessage() =>
        InvalidFormat;

    /// <summary>
    /// Creates a validation error for invalid checksum
    /// </summary>
    /// <returns>The error message</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetInvalidChecksumMessage() =>
        InvalidChecksum;
}
