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

public class Definition : IEquatable<Definition>
{
    public string Name { get; }

    public IEnumerable<Relation> Relations { get; }

    public IEnumerable<Permission> Permissions { get; }

    public Definition(string name, IEnumerable<Relation> relations,
        IEnumerable<Permission> permissions)
    {
        Name = name;
        Relations = relations;
        Permissions = permissions;
    }

    public bool Equals(Definition? other)
    {
        if (ReferenceEquals(null, other)) { return false; }

        if (ReferenceEquals(this, other)) { return true; }

        return Name == other.Name && Relations.SequenceEqual(other.Relations) &&
               Permissions.SequenceEqual(other.Permissions);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) { return false; }

        if (ReferenceEquals(this, obj)) { return true; }

        if (obj.GetType() != this.GetType()) { return false; }

        return Equals((Definition)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Name, Relations, Permissions);
}

public interface IDefinitionMember
{
    string Name { get; }
}

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