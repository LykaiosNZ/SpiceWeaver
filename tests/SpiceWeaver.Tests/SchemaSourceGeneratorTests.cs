using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Snapshooter;

namespace SpiceWeaver.Tests;

[TestFixture(TestOf = typeof(SchemaSourceGenerator))]
public sealed class SchemaSourceGeneratorTests
{
    [Test]
    public void SingleSchema()
    {
        var fooSchema = new StringAdditionalText("foo", TestSchema.SpiceDb);

        IEnumerable<KeyValuePair<string, TestAnalyzerConfigOptions>> additionalTextOptions =
        [
            CreateSchemaFileOptions(fooSchema.Path)
        ];

        var optionsProvider =
            new TestAnalyzerConfigOptionsProvider(TestAnalyzerConfigOptions.Empty, additionalTextOptions);

        ExecuteGenerator(new[] { fooSchema }, optionsProvider, out var result);

        result.Results[0].GeneratedSources.Should().HaveCount(1);
        result.Results[0].GeneratedSources[0].HintName.Should().Be("SpiceWeaver.Schema.g.cs");

        Snapshot.Match(result.Results[0].GeneratedSources[0].SourceText.ToString());
    }

    [Test]
    public void MultipleSchemas()
    {
        var fooSchema = new StringAdditionalText("foo", TestSchema.SpiceDb);
        var barSchema = new StringAdditionalText("bar", TestSchema.SpiceDb);

        IEnumerable<KeyValuePair<string, TestAnalyzerConfigOptions>> additionalTextOptions =
        [
            CreateSchemaFileOptions(fooSchema.Path, className: "Foo"),
            CreateSchemaFileOptions(barSchema.Path, className: "Bar")
        ];

        var optionsProvider =
            new TestAnalyzerConfigOptionsProvider(TestAnalyzerConfigOptions.Empty, additionalTextOptions);

        ExecuteGenerator(new[] { fooSchema, barSchema }, optionsProvider, out var result);

        result.Results[0].GeneratedSources.Should().HaveCount(2);
        result.Results[0].GeneratedSources[0].HintName.Should().Be(("SpiceWeaver.Foo.g.cs"));
        result.Results[0].GeneratedSources[1].HintName.Should().Be(("SpiceWeaver.Bar.g.cs"));

        Snapshot.Match(result.Results[0].GeneratedSources[0].SourceText.ToString(), new SnapshotNameExtension("foo"));
        Snapshot.Match(result.Results[0].GeneratedSources[1].SourceText.ToString(), new SnapshotNameExtension("bar"));
    }

    [Test]
    public void SingleSchema_Json()
    {
        var fooSchema = new StringAdditionalText("foo", TestSchema.Json);

        IEnumerable<KeyValuePair<string, TestAnalyzerConfigOptions>> additionalTextOptions =
        [
            CreateSchemaFileOptions(fooSchema.Path, isJson: true)
        ];

        var optionsProvider =
            new TestAnalyzerConfigOptionsProvider(TestAnalyzerConfigOptions.Empty, additionalTextOptions);

        ExecuteGenerator(new[] { fooSchema }, optionsProvider, out var result);

        result.Results[0].GeneratedSources.Should().HaveCount(1);
        result.Results[0].GeneratedSources[0].HintName.Should().Be("SpiceWeaver.Schema.g.cs");

        Snapshot.Match(result.Results[0].GeneratedSources[0].SourceText.ToString());
    }

    [Test]
    public void MultipleSchemas_Mixed()
    {
        var fooSchema = new StringAdditionalText("foo", TestSchema.SpiceDb);
        var barSchema = new StringAdditionalText("bar", TestSchema.Json);

        IEnumerable<KeyValuePair<string, TestAnalyzerConfigOptions>> additionalTextOptions =
        [
            CreateSchemaFileOptions(fooSchema.Path, className: "Foo"),
            CreateSchemaFileOptions(barSchema.Path, className: "Bar", isJson: true)
        ];

        var optionsProvider =
            new TestAnalyzerConfigOptionsProvider(TestAnalyzerConfigOptions.Empty, additionalTextOptions);

        ExecuteGenerator(new[] { fooSchema, barSchema }, optionsProvider, out var result);

        result.Results[0].GeneratedSources.Should().HaveCount(2);
        result.Results[0].GeneratedSources[0].HintName.Should().Be(("SpiceWeaver.Foo.g.cs"));
        result.Results[0].GeneratedSources[1].HintName.Should().Be(("SpiceWeaver.Bar.g.cs"));

        Snapshot.Match(result.Results[0].GeneratedSources[0].SourceText.ToString(), new SnapshotNameExtension("foo"));
        Snapshot.Match(result.Results[0].GeneratedSources[1].SourceText.ToString(), new SnapshotNameExtension("bar"));
    }

    [Test]
    public void InvalidSchema()
    {
        var fooSchema = new StringAdditionalText("foo", "This is not a valid schema.");

        IEnumerable<KeyValuePair<string, TestAnalyzerConfigOptions>> additionalTextOptions =
            [CreateSchemaFileOptions(fooSchema.Path)];

        var optionsProvider =
            new TestAnalyzerConfigOptionsProvider(TestAnalyzerConfigOptions.Empty, additionalTextOptions);

        ExecuteGenerator(new[] { fooSchema }, optionsProvider, out var result);

        result.Diagnostics.Should().HaveCount(1);
        result.Diagnostics[0].Descriptor.Should().Be(DiagnosticsDescriptors.Spice2JsonError);
        result.Diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Test]
    [TestCase]
    public void InvalidSchema_Json()
    {
        var fooSchema = new StringAdditionalText("foo", "This is not a valid schema.");

        IEnumerable<KeyValuePair<string, TestAnalyzerConfigOptions>> additionalTextOptions =
            [CreateSchemaFileOptions(fooSchema.Path, isJson: true)];

        var optionsProvider =
            new TestAnalyzerConfigOptionsProvider(TestAnalyzerConfigOptions.Empty, additionalTextOptions);

        ExecuteGenerator(new[] { fooSchema }, optionsProvider, out var result);

        result.Diagnostics.Should().HaveCount(1);
        result.Diagnostics[0].Descriptor.Should().Be(DiagnosticsDescriptors.DeserializationError);
        result.Diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptySchema(bool isJson)
    {
        var fooSchema = new StringAdditionalText("foo", string.Empty);

        IEnumerable<KeyValuePair<string, TestAnalyzerConfigOptions>> additionalTextOptions =
            [CreateSchemaFileOptions(fooSchema.Path, isJson: isJson)];

        var optionsProvider =
            new TestAnalyzerConfigOptionsProvider(TestAnalyzerConfigOptions.Empty, additionalTextOptions);

        ExecuteGenerator(new[] { fooSchema }, optionsProvider, out var result);

        result.Diagnostics.Should().HaveCount(1);
        result.Diagnostics[0].Descriptor.Should().Be(DiagnosticsDescriptors.EmptyFileError);
        result.Diagnostics[0].Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    private static void ExecuteGenerator(IEnumerable<AdditionalText> additionalTexts,
        AnalyzerConfigOptionsProvider optionsProvider,
        out GeneratorDriverRunResult runResult)
    {
        Compilation inputCompilation = CreateCompilation();

        GeneratorDriver driver = CSharpGeneratorDriver.Create([new SchemaSourceGenerator()]);

        driver = driver
            .AddAdditionalTexts(additionalTexts.ToImmutableArray())
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation _,
                out ImmutableArray<Diagnostic> _);

        runResult = driver.GetRunResult();
    }

    private static KeyValuePair<string, TestAnalyzerConfigOptions> CreateSchemaFileOptions(string path,
        bool? isJson = default,
        string? @namespace = default,
        string? className = default)
    {
        var options = new Dictionary<string, string?>
        {
            [SchemaSourceGenerator.SchemaFileOptionKey] = true.ToString()
        };

        if (isJson is not null) { options[SchemaSourceGenerator.IsJsonOptionKey] = isJson.ToString(); }

        if (@namespace is not null) { options[SchemaSourceGenerator.NamespaceOptionKey] = @namespace; }

        if (className is not null) { options[SchemaSourceGenerator.ClassNameOptionKey] = className; }

        return new KeyValuePair<string, TestAnalyzerConfigOptions>(path, new TestAnalyzerConfigOptions(options));
    }

    private static Compilation CreateCompilation()
        => CSharpCompilation.Create("compilation",
            new[] { CSharpSyntaxTree.ParseText(string.Empty) },
            new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}