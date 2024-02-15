using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace SpiceWeaver;

[SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking")]
public static class DiagnosticsDescriptors
{
    public static readonly DiagnosticDescriptor Spice2JsonError = new(id: "SPCWVR001",
        title: "Error converting schema to json",
        messageFormat: "Schema file: {0}, Exception Message: {2}",
        category: nameof(SchemaSourceGenerator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor DeserializationError = new(id: "SPCWVR002",
        title: "Error deserializing schema json",
        messageFormat: "Schema file: {0}, Exception Message: {2}",
        category: nameof(SchemaSourceGenerator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor EmptyFileError = new(id: "SPCWVR003",
        title: "Schema file empty",
        messageFormat: "Schema file {0} is empty",
        category: nameof(SchemaSourceGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}