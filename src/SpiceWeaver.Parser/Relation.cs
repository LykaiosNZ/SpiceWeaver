using System;

namespace SpiceWeaver.Parser;

public class Relation : IEquatable<Relation>, IDefinitionMember
{
    public string Name { get; }

    public string Expression { get; }

    public Relation(string name, string expression)
    {
        Name = name;
        Expression = expression;
    }

    public bool Equals(Relation? other)
    {
        if (ReferenceEquals(null, other)) { return false; }

        if (ReferenceEquals(this, other)) { return true; }

        return Name == other.Name && Expression == other.Expression;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) { return false; }

        if (ReferenceEquals(this, obj)) { return true; }

        if (obj.GetType() != this.GetType()) { return false; }

        return Equals((Relation)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Name, Expression);
}