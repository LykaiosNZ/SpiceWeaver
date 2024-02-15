using System;

namespace SpiceWeaver;

public class SpiceWeaverException : Exception
{
    public SpiceWeaverException(string message) : base(message) { }

    public SpiceWeaverException(string message, Exception innerException) : base(message, innerException) { }
}

public class Spice2JsonException : SpiceWeaverException
{
    public Spice2JsonException(string message) : base(message) { }

    public Spice2JsonException(string message, Exception innerException) : base(message, innerException) { }
}