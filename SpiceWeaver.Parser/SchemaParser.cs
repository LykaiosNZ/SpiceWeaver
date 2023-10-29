using System.Collections.Generic;
using Pidgin;
using System.Linq;
using Pidgin.Comment;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using static ZedParser.Tokens;

namespace ZedParser;

public static class SchemaParser
{
    // Character sets
    private static readonly Parser<char, char> _lowercaseOrDigit = Lowercase.Or(Digit);
    private static readonly Parser<char, char> _lowercaseOrDigitOrUnderscore = _lowercaseOrDigit.Or(Char('_'));

    private static readonly Parser<char, string> _identifier = Tok(_lowercaseOrDigitOrUnderscore.AtLeastOnceString());

    // Relations
    private static readonly Parser<char, string> _relationName = _identifier.Labelled("relationName");

    private static readonly Parser<char, string> _relationExpression =
        Tok(Map(
                (first, rest) => first + rest,
                Lowercase,
                OneOf(
                        _lowercaseOrDigitOrUnderscore, Char('#'), Char(':'), Char('*'), Char('|'), Char(' ')
                    )
                    .ManyString()
            ))
            .Labelled("relationExpression");

    private static readonly Parser<char, Relation> _relation =
        Tok("relation")
            .Then(
                Map(
                    (name, expression) => new Relation(name.Trim(), expression.Trim()),
                    _relationName.Before(Colon),
                    _relationExpression
                )
            )
            .Labelled("relation");

    // Permissions
    private static readonly Parser<char, string> _permissionName = _identifier.Labelled("permissionName");

    private static readonly Parser<char, string> _permissionExpression =
        Tok(Map(
                (first, rest) => first + rest,
                Lowercase,
                OneOf(
                        _lowercaseOrDigitOrUnderscore, Char('-'), Char('>'), Char('+'), Char(' '), Char('-'), Char('&')
                    )
                    .ManyString()
            ))
            .Labelled("permissionExpression");

    private static readonly Parser<char, Permission> _permission =
        Tok("permission")
            .Then(
                Map(
                    (name, expression) => new Permission(name.Trim(), expression.Trim()),
                    _permissionName.Before(Equal),
                    _permissionExpression
                )
            );

    // Definitions
    private static readonly Parser<char, string> _definitionName = _identifier.Labelled("definitionName");

    private static readonly Parser<char, IDefinitionMember> _definitionMember =
        OneOf(_relation.Cast<IDefinitionMember>(), _permission.Cast<IDefinitionMember>()).Before(SkipWhitespaces);

    private static readonly Parser<char, Definition> _definition =
        Tok("definition")
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
                    _definitionMember.Many().Before(CloseBrace)
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

    public static Result<char, Schema> Parse(string input) =>
        StripComments(input) switch
        {
            { Success: true } success => _schema.Parse(success.Value),
            { } failed => failed.Cast<Schema>()
        };

    private static Result<char, string> StripComments(string input) => _stripComments.Parse(input);
}