using SpiceWeaver.Parser;

namespace SpiceWeaver.Tests.Parser;

public class SchemaParserTests
{
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("\r\n")]
    public void EmptySchema(string input)
    {
        var result = SchemaParser.Parse(input);

        result.Should().BeNull();
    }

    [Test]
    public void SingleDefinition()
    {
        var input = "definition user {}";
        var expected = new Schema(new[] { EmptyDefinition("user") });

        AssertEquivalent(input, expected);
    }

    [Test]
    public void MultipleDefinitions()
    {
        var input = """
                    definition user {}
                    definition organization {}
                    """;

        var expected = new Schema(new[] { EmptyDefinition("user"), EmptyDefinition("organization") });

        AssertEquivalent(input, expected);
    }

    [TestCase("definition {}", TestName = "{m}_MissingName")]
    [TestCase("defin!ition foo {}", TestName = "{m}_MisspelledKeyword")]
    [TestCase("definition foo { definition bar {} }", TestName = "{m}_NestedDefinition")]
    [TestCase("definition document { foo permission }", TestName = "{m}_UnknownKeywordInBody")]
    public void MalformedDefinition(string input)
    {
        var result = SchemaParser.Parse(input);

        result.Should().BeNull();
    }

    [TestCase("{}")]
    [TestCase("{   }")]
    [TestCase("\r\n{}")]
    [TestCase("{\r\n}")]
    [TestCase("{}\r\n")]
    public void DefinitionBraceLayout(string layout)
    {
        var input = $"definition user {layout}";
        var expected = new Schema(new[] { EmptyDefinition("user") });

        AssertEquivalent(input, expected);
    }

    [Test]
    public void SingleRelation()
    {
        var input = """
                    definition user {}

                    definition organization {
                        relation member : user
                    }
                    """;

        var expected = new Schema(new[]
        {
            EmptyDefinition("user"),
            new Definition("organization", new[] { new Relation("member", "user") }, Enumerable.Empty<Permission>())
        });

        AssertEquivalent(input, expected);
    }

    [Test]
    public void MultipleRelations()
    {
        var input = """
                    definition document {
                        relation viewers : user
                        relation editors: user
                    }
                    """;

        var expected = new Schema(new[]
        {
            new Definition("document", new[]
                {
                    new Relation("viewers", "user"),
                    new Relation("editors", "user")
                },
                Enumerable.Empty<Permission>()),
        });

        AssertEquivalent(input, expected);
    }

    [Test]
    public void SinglePermission()
    {
        var input = """
                    definition document {
                        permission view = viewers
                    }
                    """;

        var expected = new Schema(new[]
        {
            new Definition("document", Enumerable.Empty<Relation>(), new[] { new Permission("view", "viewers") })
        });

        AssertEquivalent(input, expected);
    }

    [Test]
    public void MultiplePermissions()
    {
        var input = """
                    definition document {
                        permission view = viewers
                        permission edit = editors
                    }
                    """;

        var expected = new Schema(new[]
        {
            new Definition("document", Enumerable.Empty<Relation>(), new[]
            {
                new Permission("view", "viewers"),
                new Permission("edit", "editors")
            })
        });

        AssertEquivalent(input, expected);
    }

    [Test]
    public void InterleavedRelationsAndPermissions()
    {
        var input = """
                    definition document {
                        permission view = viewer
                        relation viewer: user
                        relation editor: user
                        permission edit = editor
                    }
                    """;

        var expected = new Schema(new[]
        {
            new Definition("document", new[]
            {
                new Relation("viewer", "user"),
                new Relation("editor", "user")
            }, new[]
            {
                new Permission("view", "viewer"),
                new Permission("edit", "editor")
            })
        });

        AssertEquivalent(input, expected);
    }

    [Test]
    public void ComplexSchema()
    {
        var input = """
                    definition user {}

                    definition organization {
                        relation teacher: user
                        relation all_classes_viewers: organization#teacher
                    }

                    definition class {
                        relation organization: organization
                        relation teacher: user
                    
                        permission view = teacher + organisation->all_classes_viewers
                    }
                    """;

        var expected = new Schema(new[]
        {
            new Definition("user", Enumerable.Empty<Relation>(), Enumerable.Empty<Permission>()),

            new Definition(
                "organization", new[]
                {
                    new Relation("teacher", "user"),
                    new Relation("all_classes_viewers", "organization#teacher")
                },
                Enumerable.Empty<Permission>()
            ),

            new Definition(
                "class",
                new[]
                {
                    new Relation("organization", "organization"),
                    new Relation("teacher", "user")
                },
                new[]
                {
                    new Permission("view", "teacher + organisation->all_classes_viewers")
                }
            )
        });

        AssertEquivalent(input, expected);
    }

    [Test]
    public void IgnoresComments()
    {
        var input = """
                    definition organization {}

                    // A definition
                    definition document { /*  block comment */ } // A comment at the end of the line
                    """;

        var expected = new Schema(new[]
        {
            EmptyDefinition("organization"), EmptyDefinition("document")
        });

        var result = SchemaParser.Parse(input);

        result.Should().Be(expected);
    }

    [Test]
    public void BlockCommentEndMissing()
    {
        var input = """
                    definition organization {}
                    /* This is missing an end block
                    definition document { }
                    """;

        var result = SchemaParser.Parse(input);

        result.Should().BeNull();
    }

    private static IEnumerable<TestCaseData> ValidIdentifiers => new[]
    {
        new TestCaseData("a").SetName("{m}_SingleLowercase"),
        new TestCaseData("abc").SetName("{m}_MultipleLowercase"),
        new TestCaseData("a_c").SetName("{m}_ContainsUnderscore"),
        new TestCaseData("a1c").SetName("{m}_ContainsDigit"),
        new TestCaseData("ab1").SetName("{m}_EndsWithDigit"),
        new TestCaseData(string.Join("", Enumerable.Repeat("a", 64))).SetName("{m}_MaximumValidLength")
    };

    [TestCaseSource(nameof(ValidIdentifiers))]
    public void ValidRelationName(string identifier)
    {
        var input = $"definition document {{ relation {identifier}: foo }}";

        var expected = new Schema(new[]
        {
            new Definition("document", new[]
            {
                new Relation(identifier, "foo")
            }, Enumerable.Empty<Permission>())
        });

        AssertEquivalent(input, expected);
    }

    [TestCaseSource(nameof(ValidIdentifiers))]
    public void ValidPermissionName(string identifier)
    {
        var input = $"definition document {{ permission {identifier} = foo }}";

        var expected = new Schema(new[]
        {
            new Definition("document", Enumerable.Empty<Relation>(), new[]
            {
                new Permission(identifier, "foo")
            })
        });

        AssertEquivalent(input, expected);
    }

    private static IEnumerable<TestCaseData> InvalidIdentifiers => new[]
    {
        new TestCaseData("aBc").SetName("{m}_ContainsUppercase"),
        new TestCaseData("1a").SetName("{m}_StartsWithDigit"),
        new TestCaseData("_a").SetName("{m}_StartsWithUnderscore"),
        new TestCaseData("a_").SetName("{m}_EndsWithUnderscore"),
        new TestCaseData("a c").SetName("{m}_ContainsWhiteSpace"),
        new TestCaseData("!bc").SetName("{m}_StartsWithInvalidChar"),
        new TestCaseData("a!c").SetName("{m}_ContainsInvalidChar"),
        new TestCaseData("ab!").SetName("{m}_EndsWithInvalidChar"),
        new TestCaseData(string.Join("", Enumerable.Repeat("a", 65))).SetName("{m}_GreaterThan64Chars")
    };

    [TestCaseSource(nameof(InvalidIdentifiers))]
    public void InvalidRelationName(string name)
    {
        var input = $"definition document {{ relation {name}: foo }}";

        var result = SchemaParser.Parse(input);

        result.Should().BeNull();
    }

    [TestCaseSource(nameof(InvalidIdentifiers))]
    public void InvalidPermissionName(string name)
    {
        var input = $"definition document {{ permission {name} = foo }}";

        var result = SchemaParser.Parse(input);

        result.Should().BeNull();
    }

    private static void AssertEquivalent(string input, Schema expected)
    {
        var result = SchemaParser.Parse(input);

        result.Should().BeEquivalentTo(expected);
    }

    private static Definition EmptyDefinition(string name) =>
        new(name, Enumerable.Empty<Relation>(), Enumerable.Empty<Permission>());
}