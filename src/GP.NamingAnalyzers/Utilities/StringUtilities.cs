// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Text.RegularExpressions;

namespace GP.NamingAnalyzers.Utilities;

/// <summary>
/// Contains utilities methods for strings.
/// </summary>
public static class StringUtilities
{
    /// <summary>
    /// Returns a value indicating whether a string is a valid regex pattern.
    /// </summary>
    /// <param name="regexPattern">The regex pattern to verify.</param>
    /// <returns><see langword="true" /> if the string is a valid regex pattern; otherwise, <see langword="false" />.</returns>
    public static bool IsValidRegexPattern(string regexPattern)
    {
        if (string.IsNullOrWhiteSpace(regexPattern))
        {
            return false;
        }

        try
        {
            Regex.Match(string.Empty, regexPattern);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }
}
