namespace SpiceWeaver.Tests;

[TestFixture(TestOf = typeof(CodeGenerator))]
public sealed class CodeGeneratorTests
{
    [Test]
    [TestCaseSource(nameof(NullOrWhitespace))]
    public void GenerateFromJson_WhenSchemaJsonIsNullOrWhitespace_ShouldThrow(string? json)
    {
        Assert.Throws<ArgumentException>(() => CodeGenerator.Generate("foo", "bar", json!));
    }

    [Test]
    [TestCaseSource(nameof(NullOrWhitespace))]
    public void GenerateFromJson_WhenNamespaceIsNullOrWhitespace_ShouldThrow(string? @namespace)
    {
        Assert.Throws<ArgumentException>(() => CodeGenerator.Generate(@namespace!, "bar", TestSchema.Json));
    }
    
    [Test]
    [TestCaseSource(nameof(NullOrWhitespace))]
    public void GenerateFromJson_WhenClassNameIsNullOrWhitespace_ShouldThrow(string? className)
    {
        Assert.Throws<ArgumentException>(() => CodeGenerator.Generate("foo", className!, TestSchema.Json));
    }

    [Test]
    public void GenerateFromJson_ShouldGenerateExpectedOutput()
    {
        var output = CodeGenerator.Generate("TestNamespace", "TestSchema", TestSchema.Json);
        
        Snapshot.Match(output);
    }

    [Test]
    [TestCaseSource(nameof(NullOrWhitespace))]
    public void GenerateFromSchema_WhenNamespaceIsNullOrWhitespace_ShouldThrow(string? @namespace)
    {
        Assert.Throws<ArgumentException>(() => CodeGenerator.Generate(@namespace!, "bar", TestSchema.Object));
    }
    
    [Test]
    [TestCaseSource(nameof(NullOrWhitespace))]
    public void GenerateFromSchema_WhenClassNameIsNullOrWhitespace_ShouldThrow(string? className)
    {
        Assert.Throws<ArgumentException>(() => CodeGenerator.Generate("foo", className!, TestSchema.Object));
    }

    [Test]
    public void GenerateFromSchema_ShouldGenerateExpectedOutput()
    {
        var output = CodeGenerator.Generate("TestNamespace", "TestSchema", TestSchema.Object);
        
        Snapshot.Match(output);
    }

    private static IEnumerable<string?> NullOrWhitespace =>
    [
        null,
        "",
        " ",
        "\r",
        "\t"
    ];
}