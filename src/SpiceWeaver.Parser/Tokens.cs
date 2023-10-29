using Pidgin;
using static Pidgin.Parser;

namespace SpiceWeaver.Parser;

internal static class Tokens
{
    public static readonly Parser<char, char> OpenBrace = Tok('{');
    public static readonly Parser<char, char> CloseBrace = Tok('}');
    public static readonly Parser<char, char> Colon = Tok(':');
    public static readonly Parser<char, char> Equal = Tok('=');
    
    public static Parser<char, T> Tok<T>(Parser<char, T> p) => Try(p).Before(SkipWhitespaces);
    public static Parser<char, char> Tok(char value) => Tok(Char(value));
    public static Parser<char, string> Tok(string value) => Tok(String(value));
}