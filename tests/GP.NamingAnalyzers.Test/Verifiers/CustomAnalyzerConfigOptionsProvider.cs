// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GP.NamingAnalyzers.Test.Verifiers;

public class CustomAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly Dictionary<string, string> _optionValuesByOptionName;

    public override AnalyzerConfigOptions GlobalOptions { get; }
        = new CustomAnalyzerConfigOptions(new Dictionary<string, string>());

    public CustomAnalyzerConfigOptionsProvider(Dictionary<string, string> optionValuesByOptionName) =>
        _optionValuesByOptionName = optionValuesByOptionName;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
        new CustomAnalyzerConfigOptions(_optionValuesByOptionName);

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
        new CustomAnalyzerConfigOptions(new Dictionary<string, string>());
}
