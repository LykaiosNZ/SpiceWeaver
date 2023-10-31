using System.Diagnostics;

namespace SpiceWeaver.Parser;

public class ParseResult
{
    public ParseError? Error { get; }

    public bool WasSuccessful { get; }

    public Schema? Value { get; }

    private ParseResult(Schema value)
    {
        Value = value;
        WasSuccessful = true;
    }

    private ParseResult(ParseError error) { Error = error; }

    public static ParseResult Failure(string message, int column, int line) =>
        Failure(new ParseError(message, column, line));

    public static ParseResult Failure(ParseError error) => new(error);
    public static ParseResult Success(Schema value) => new(value);
}

public class ParseError
{
    public int Column { get; }
    public int Line { get; }
    public string Message { get; }

    public ParseError(string message, int column, int line)
    {
        Message = message;
        Column = column;
        Line = line;
    }
}