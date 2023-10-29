using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceWeaver.Parser;

public class Schema : IEquatable<Schema>
{
    public IEnumerable<Definition> Definitions { get; }

    public Schema(IEnumerable<Definition> definitions) { Definitions = definitions; }

    public bool Equals(Schema? other)
    {
        if (ReferenceEquals(null, other)) { return false; }

        if (ReferenceEquals(this, other)) { return true; }

        return Definitions.SequenceEqual(other.Definitions);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) { return false; }

        if (ReferenceEquals(this, obj)) { return true; }

        if (obj.GetType() != this.GetType()) { return false; }

        return Equals((Schema)obj);
    }

    public override int GetHashCode() => Definitions.GetHashCode();
}