using System;

namespace SpiceWeaver.Parser;

public class Permission : IEquatable<Permission>, IDefinitionMember
{
    public string Name { get; }

    public string Expression { get; }

    public Permission(string name, string expression)
    {
        Name = name;
        Expression = expression;
    }

    public bool Equals(Permission? other)
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

        return Equals((Permission)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Name, Expression);
}