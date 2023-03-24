// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;

namespace GP.NamingAnalyzers.Test.Verifiers;

public class CustomAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly Dictionary<string, string> _optionValuesByOptionName;

    public CustomAnalyzerConfigOptions(Dictionary<string, string> optionValuesByOptionName) =>
        _optionValuesByOptionName = optionValuesByOptionName;

    public override bool TryGetValue(string key, out string value) =>
        _optionValuesByOptionName.TryGetValue(key, out value!);
}
