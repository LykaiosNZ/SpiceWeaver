using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceWeaver.Parser;

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