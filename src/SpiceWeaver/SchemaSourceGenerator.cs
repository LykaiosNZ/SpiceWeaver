using System;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[assembly: InternalsVisibleTo("SpiceWeaver.Tests")]

namespace SpiceWeaver;

[Generator]
public sealed class SchemaSourceGenerator : IIncrementalGenerator
{
    private const string OptionPrefix = "SpiceWeaver_";
    private const string BuildPropertyPrefix = $"build_property.{OptionPrefix}";
    private const string AdditionalFileMetadataPrefix = $"build_metadata.AdditionalFiles.{OptionPrefix}";

    internal const string ClassNameOptionKey = $"{AdditionalFileMetadataPrefix}ClassName";
    internal const string IsJsonOptionKey = $"{AdditionalFileMetadataPrefix}IsJson";
    internal const string NamespaceOptionKey = $"{AdditionalFileMetadataPrefix}Namespace";
    internal const string SchemaFileOptionKey = $"{AdditionalFileMetadataPrefix}SchemaFile";
    internal const string Spice2JsonPathOptionKey = $"{BuildPropertyPrefix}Spice2JsonPath";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var schemasAndOptionsProvider = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Where(textAndOptions =>
            {
                var options = textAndOptions.Right.GetOptions(textAndOptions.Left);

                return options.TryGetValue(SchemaFileOptionKey, out var isSchemaFile)
                       && isSchemaFile.Equals("true", StringComparison.OrdinalIgnoreCase);
            });

        var schemaFiles = schemasAndOptionsProvider.Select((textAndOptions, ct) =>
        {
            (AdditionalText additionalText, AnalyzerConfigOptionsProvider optionsProvider) = textAndOptions;

            var content = additionalText.GetText(ct)!.ToString();
            var schemaFile = new SchemaFile(additionalText.Path, content);
            var additionalTextOptions = optionsProvider.GetOptions(additionalText);

            if (optionsProvider.GlobalOptions.TryGetNotNullOrEmptyValue(Spice2JsonPathOptionKey,
                    out var spice2Jsonpath)) { schemaFile.Spice2JsonPath = spice2Jsonpath!; }

            if (additionalTextOptions.TryGetValue(IsJsonOptionKey, out var isJson))
            {
                schemaFile.IsJsonFile = isJson.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            if (additionalTextOptions.TryGetNotNullOrEmptyValue(NamespaceOptionKey, out var generatedNamespace))
            {
                schemaFile.Namespace = generatedNamespace!;
            }

            if (additionalTextOptions.TryGetNotNullOrEmptyValue(ClassNameOptionKey, out var className))
            {
                schemaFile.ClassName = className!;
            }

            return schemaFile;
        });

        context.RegisterSourceOutput(schemaFiles, (productionContext, schemaFile) =>
        {
            try
            {
                if (schemaFile.Contents.Length is 0)
                {
                    productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticsDescriptors.EmptyFileError,
                        Location.None, schemaFile.FileName));

                    return;
                }

                var json = schemaFile.IsJsonFile
                    ? schemaFile.Contents
                    : Spice2Json.ConvertToJson(schemaFile.Spice2JsonPath, schemaFile.Contents);

                productionContext.AddSource($"{schemaFile.Namespace}.{schemaFile.ClassName.ToPascalCase()}.g.cs",
                    CodeGenerator.Generate(schemaFile.Namespace, schemaFile.ClassName, json));
            }
            catch (Spice2JsonException e)
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticsDescriptors.Spice2JsonError,
                    Location.None, schemaFile.FileName, e.Message));
            }
            catch (SpiceWeaverException e)
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticsDescriptors.DeserializationError,
                    Location.None, schemaFile.FileName, e.Message));
            }
        });
    }

    private sealed class SchemaFile
    {
        public SchemaFile(string fileName, string contents)
        {
            FileName = fileName;
            Contents = contents;
        }

        public string FileName { get; }
        public string Contents { get; }
        public bool IsJsonFile { get; set; }
        public string Spice2JsonPath { get; set; } = "spice2json";
        public string Namespace { get; set; } = "SpiceWeaver";
        public string ClassName { get; set; } = "Schema";
    }
}