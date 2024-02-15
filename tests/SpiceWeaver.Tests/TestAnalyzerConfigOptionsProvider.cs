using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SpiceWeaver.Tests;

public sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly Dictionary<string, TestAnalyzerConfigOptions> _additionalTextOptions;

    public TestAnalyzerConfigOptionsProvider(TestAnalyzerConfigOptions globalOptions,
        IEnumerable<KeyValuePair<string, TestAnalyzerConfigOptions>> additionalTextOptions)
    {
        GlobalOptions = globalOptions;
        _additionalTextOptions = additionalTextOptions.ToDictionary();
    }

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => throw new NotImplementedException();

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
        _additionalTextOptions.TryGetValue(textFile.Path, out var options) ? options : TestAnalyzerConfigOptions.Empty;

    public override AnalyzerConfigOptions GlobalOptions { get; }
}

public sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    public static readonly TestAnalyzerConfigOptions Empty = new([]);

    private readonly Dictionary<string, string?> _options;

    public TestAnalyzerConfigOptions(IEnumerable<KeyValuePair<string, string?>> options)
    {
        _options = options.ToDictionary();
    }

    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
        _options.TryGetValue(key, out value);
}