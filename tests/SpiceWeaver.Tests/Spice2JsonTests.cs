namespace SpiceWeaver.Tests;

[TestFixture(TestOf = typeof(Spice2Json))]
public sealed class Spice2JsonTests
{
    [Test]
    public void ConvertToJson_WhenPathIsInvalid_ShouldThrow()
    {
        var e = Assert.Throws<Spice2JsonException>(() => Spice2Json.ConvertToJson("someFakePath", TestSchema.SpiceDb));

        Snapshot.Match(e?.Message);
    }

    [Test]
    public void ConvertToJson_WhenSchemaFileIsInvalid_ShouldThrow()
    {
        var e = Assert.Throws<Spice2JsonException>(
            () => Spice2Json.ConvertToJson("spice2json", "This is not a schema."));

        Snapshot.Match(e?.Message);
    }

    [Test]
    public void ConvertToJson_ShouldGenerateExpectedOutput()
    {
        var result = Spice2Json.ConvertToJson("spice2json", TestSchema.SpiceDb);
        
        Snapshot.Match(result);
    }
}