using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SpiceWeaver;

public sealed class Schema
{
    [JsonPropertyName("definitions")] public List<Definition> Definitions { get; set; } = new();
}

public sealed class Definition
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("namespace")] public string? Namespace { get; set; }
    [JsonPropertyName("relations")] public List<Relation> Relations { get; set; } = new();
    [JsonPropertyName("permissions")] public List<Permission> Permissions { get; set; } = new();
    [JsonPropertyName("comment")] public string? Comment { get; set; }
}

public sealed class Relation
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("types")] public List<RelationType> Types { get; set; } = new();
    [JsonPropertyName("comment")] public string? Comment { get; set; }
}

public sealed class RelationType
{
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("relation")] public string? Relation { get; set; }
    [JsonPropertyName("comment")] public string? Comment { get; set; }
}

public sealed class Permission
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("userSet")] public UserSet UserSet { get; set; }
    [JsonPropertyName("comment")] public string? Comment { get; set; }
}

public sealed class UserSet
{
    [JsonPropertyName("operation")] public string? Operation { get; set; }
    [JsonPropertyName("relation")] public string? Relation { get; set; }
    [JsonPropertyName("permission")] public string? Permission { get; set; }
    [JsonPropertyName("children")] public List<UserSet> Children { get; set; } = new();
}
