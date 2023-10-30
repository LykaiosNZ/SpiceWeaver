using Pidgin;
using Pidgin.Comment;
using static Pidgin.Parser;

namespace SpiceWeaver.Parser;

internal static class UtilityParsers
{
    public static readonly Parser<char, char> OpenBrace = Tok('{');
    public static readonly Parser<char, char> CloseBrace = Tok('}');
    public static readonly Parser<char, char> Colon = Tok(':');
    public static readonly Parser<char, char> Equal = Tok('=');

    public static readonly Parser<char, string> Identifier =
        Tok(
            Map(
                    (first, rest) => first + rest,
                    Lowercase,
                    OneOf(Lowercase, Digit, Char('_')).ManyString()
                )
                // Easier to assert this than leaning on parsers to do it
                .Assert(s => s.Length <= 64 && !s.EndsWith('_'))
        );

    public static readonly Parser<char, Unit> SkipWhiteSpacesAndComments =
        OneOf(
                CommentParser.SkipLineComment(Try(String("//"))).Before(SkipWhitespaces),
                CommentParser.SkipBlockComment(Try(String("/*")), String("*/")).Before(SkipWhitespaces)
            )
            .SkipMany()
            .Between(SkipWhitespaces);


    public static Parser<char, string> Keyword(string keyword) => Tok(keyword);

    // https://www.benjamin.pizza/posts/2019-12-08-parsing-prolog-with-pidgin.html
    // Replace with built-in version when available
    public static Parser<char, T> Tok<T>(Parser<char, T> p) => Try(p).Before(Rec(() => SkipWhiteSpacesAndComments));
    public static Parser<char, char> Tok(char value) => Tok(Char(value));
    public static Parser<char, string> Tok(string value) => Tok(String(value));
}