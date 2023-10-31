using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Pidgin;
using static Pidgin.Parser;
using static SpiceWeaver.Parser.UtilityParsers;

namespace SpiceWeaver.Parser;

public static class SchemaParser
{
    // Relations
    private static readonly Parser<char, string> _relationName = Identifier.Labelled("relationName");

    private static readonly Parser<char, string> _relationExpression =
        Tok(
                Map(
                    (first, rest, _) => first + rest,
                    Lowercase,
                    OneOf(
                            Lowercase, Digit, Char('_'), Char('#'), Char(':'), Char('*'), Char('|'), Char(' ')
                        )
                        .ManyString(),
                    StatementTerminator
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
                (first, rest, _) => first + rest,
                Lowercase,
                OneOf(
                        Lowercase, Digit, Char('_'), Char('-'), Char('>'), Char('+'), Char(' '), Char('-'), Char('&')
                    )
                    .ManyString(),
                StatementTerminator
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
                    _definitionMember.Separated(SkipWhiteSpacesAndComments).Before(CloseBrace)
                )
            )
            .Before(SkipWhiteSpacesAndComments);

    private static readonly Parser<char, Schema> _schema =
        SkipWhiteSpacesAndComments
            .Then(_definition.AtLeastOnce())
            .Select(d => new Schema(d));

    /// <summary>
    /// Parses text representing a SpiceDB schema into a <see cref="Schema"/> instance
    /// </summary>
    /// <param name="input">Text to parse</param>
    /// <returns>A <see cref="Schema"/> instance representing the schema if parsing was successful, otherwise null</returns>
    public static ParseResult Parse(string input) => _schema.Parse(p => p.Parse(input));

    /// <summary>
    /// Parses text representing a SpiceDB schema into a <see cref="Schema"/> instance
    /// </summary>
    /// <param name="input"><see cref="TextReader"/> instance to use as source for text to parse</param>
    /// <returns>A <see cref="Schema"/> instance representing the schema if parsing was successful, otherwise null</returns>
    public static ParseResult Parse(TextReader input) => _schema.Parse(p => p.Parse(input));

    /// <summary>
    /// Attempts to Parse text representing a SpiceDB schema into a <see cref="Schema"/> instance
    /// </summary>
    /// <param name="input">Text to parse</param>
    /// <param name="schema">When this method returns, contains the schema instance if parsing was successful, otherwise null</param>
    /// <returns>True if the input was successfully parsed, otherwise false</returns>
    public static bool TryParse(string input, [NotNullWhen(true)] out Schema? schema) =>
        _schema.TryParse(p => p.Parse(input), out schema);

    /// <summary>
    /// Attempts to Parse text representing a SpiceDB schema into a <see cref="Schema"/> instance
    /// </summary>
    /// <param name="input"><see cref="TextReader"/> instance to use as source for text to parse</param>
    /// <param name="schema">When this method returns, contains the schema instance if parsing was successful, otherwise null</param>
    /// <returns>True if the input was successfully parsed, otherwise false</returns>
    public static bool TryParse(TextReader input, [NotNullWhen(true)] out Schema? schema) =>
        _schema.TryParse(p => p.Parse(input), out schema);


    private static ParseResult Parse<TToken>(this Parser<TToken, Schema> parser,
        Func<Parser<TToken, Schema>, Result<TToken, Schema>> parseFunc)
    {
        var result = parseFunc(parser);

        return result switch
        {
            { Success: true } => ParseResult.Success(result.Value),
            _ => ParseResult.Failure(
                result.Error!.RenderErrorMessage(),
                result.Error.ErrorPos.Line, result.Error.ErrorPos.Col)
        };
    }

    private static bool TryParse<TToken, T>(this Parser<TToken, T> parser,
        Func<Parser<TToken, T>, Result<TToken, T>> parseFunc, [NotNullWhen(true)] out T? output)
        where T : notnull
    {
        var result = parseFunc(parser);

        if (!result.Success)
        {
            output = default;
            return false;
        }

        output = result.Value;
        return true;
    }
}