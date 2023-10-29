using System.Collections.Generic;
using System.Linq;
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

    public static Result<char, Schema> Parse(string input) =>
        StripComments(input) switch
        {
            { Success: true } success => _schema.Parse(success.Value),
            { } failed => failed.Cast<Schema>()
        };

    private static Result<char, string> StripComments(string input) => _stripComments.Parse(input);
}