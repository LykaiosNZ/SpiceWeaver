using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SpiceWeaver.Tests;

internal sealed class StringAdditionalText : AdditionalText
{
    private readonly Lazy<SourceText> _sourceText;

    public StringAdditionalText(string path, string value)
    {
        Path = path;

        _sourceText = new Lazy<SourceText>(() => SourceText.From(value));
    }

    public override string Path { get; }

    public override SourceText GetText(CancellationToken cancellationToken = default) => _sourceText.Value;
}