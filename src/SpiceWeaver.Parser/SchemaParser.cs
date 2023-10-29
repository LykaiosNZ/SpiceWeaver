using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pidgin;
using Pidgin.Comment;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using static SpiceWeaver.Parser.Tokens;

namespace SpiceWeaver.Parser;

public static class SchemaParser
{
    // Relations
    private static readonly Parser<char, string> _relationName = Identifier.Labelled("relationName");

    private static readonly Parser<char, string> _relationExpression =
        Tok(
                Map(
                    (first, rest) => first + rest,
                    Lowercase,
                    OneOf(
                            Lowercase, Digit, Char('_'), Char('#'), Char(':'), Char('*'), Char('|'), Char(' ')
                        )
                        .ManyString()
                )
            )
            .Labelled("relationExpression");

    private static readonly Parser<char, Relation> _relation =
        Keyword("relation")
            .Then(
                Map(
                    (name, expression) => new Relation(name.Trim(), expression.Trim()),
                    _relationName.Before(Colon),
                    _relationExpression
                )
            )
            .Labelled("relation");

    // Permissions
    private static readonly Parser<char, string> _permissionName = Identifier.Labelled("permissionName");

    private static readonly Parser<char, string> _permissionExpression =
        Tok(Map(
                (first, rest) => first + rest,
                Lowercase,
                OneOf(
                        Lowercase, Digit, Char('_'), Char('-'), Char('>'), Char('+'), Char(' '), Char('-'), Char('&')
                    )
                    .ManyString()
            ))
            .Labelled("permissionExpression");

    private static readonly Parser<char, Permission> _permission =
        Keyword("permission")
            .Then(
                Map(
                    (name, expression) => new Permission(name.Trim(), expression.Trim()),
                    _permissionName.Before(Equal),
                    _permissionExpression
                )
            );

    // Definitions
    private static readonly Parser<char, string> _definitionName = Identifier.Labelled("definitionName");

    private static readonly Parser<char, IDefinitionMember> _definitionMember =
        OneOf(_relation.Cast<IDefinitionMember>(), _permission.Cast<IDefinitionMember>());

    private static readonly Parser<char, Definition> _definition =
        Keyword("definition")
            .Then(
                Map(
                    (name, members) =>
                    {
                        IEnumerable<IDefinitionMember> definitionMembers =
                            members as IDefinitionMember[] ?? members.ToArray();

                        return new Definition(name.Trim(), definitionMembers.OfType<Relation>(),
                            definitionMembers.OfType<Permission>());
                    },
                    _definitionName.Before(OpenBrace),
                    _definitionMember.Separated(SkipWhitespaces).Before(CloseBrace)
                )
            )
            .Before(SkipWhitespaces);

    private static readonly Parser<char, Schema> _schema =
        SkipWhitespaces
            .Then(_definition.AtLeastOnce())
            .Select(d => new Schema(d));

    private static readonly Parser<char, string> _stripComments =
        Any.Between(
                OneOf(
                    CommentParser.SkipBlockComment(Try(String("/*")), String("*/")),
                    CommentParser.SkipLineComment(Try(String("//"))),
                    Return(Unit.Value)
                )
            )
            .ManyString();

    /// <summary>
    /// Parses text representing a SpiceDB schema into a <see cref="Schema"/> instance
    /// </summary>
    /// <param name="input">Text to parse</param>
    /// <returns>A <see cref="Schema"/> instance representing the schema if parsing was successful, otherwise null</returns>
    public static Schema? Parse(string input) => TryParse(input, out var schema) ? schema : null;

    /// <summary>
    /// Parses text representing a SpiceDB schema into a <see cref="Schema"/> instance
    /// </summary>
    /// <param name="input"><see cref="TextReader"/> instance to use as source for text to parse</param>
    /// <returns>A <see cref="Schema"/> instance representing the schema if parsing was successful, otherwise null</returns>
    public static Schema? Parse(TextReader input) => TryParse(input, out var schema) ? schema : null;

    /// <summary>
    /// Attempts to Parse text representing a SpiceDB schema into a <see cref="Schema"/> instance
    /// </summary>
    /// <param name="input">Text to parse</param>
    /// <param name="schema">When this method returns, contains the schema instance if parsing was successful, otherwise null</param>
    /// <returns>True if the input was successfully parsed, otherwise false</returns>
    public static bool TryParse(string input, [NotNullWhen(true)] out Schema? schema)
    {
        if (_stripComments.TryParse(p => p.Parse(input), out string? stripped))
        {
            return _schema.TryParse(p => p.Parse(stripped), out schema);
        }

        schema = null;
        return false;
    }

    /// <summary>
    /// Attempts to Parse text representing a SpiceDB schema into a <see cref="Schema"/> instance
    /// </summary>
    /// <param name="input"><see cref="TextReader"/> instance to use as source for text to parse</param>
    /// <param name="schema">When this method returns, contains the schema instance if parsing was successful, otherwise null</param>
    /// <returns>True if the input was successfully parsed, otherwise false</returns>
    public static bool TryParse(TextReader input, [NotNullWhen(true)] out Schema? schema)
    {
        if (_stripComments.TryParse(p => p.Parse(input), out string? stripped))
        {
            return _schema.TryParse(p => p.Parse(stripped), out schema);
        }

        schema = default;
        return false;
    }

    private static bool TryParse<TToken, T>(this Parser<TToken, T> parser,
        Func<Parser<TToken, T>, Result<TToken, T>> parse, [NotNullWhen(true)] out T? output)
        where T : notnull
    {
        var result = parse(parser);

        if (!result.Success)
        {
            output = default;
            return false;
        }

        output = result.Value;
        return true;
    }
}