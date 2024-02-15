using System.Collections.Generic;
using Newtonsoft.Json;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SpiceWeaver;

public sealed class Schema
{
    [JsonProperty("definitions")] public List<Definition> Definitions { get; set; } = new();
}

public sealed class Definition
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("namespace")] public string? Namespace { get; set; }
    [JsonProperty("relations")] public List<Relation> Relations { get; set; } = new();
    [JsonProperty("permissions")] public List<Permission> Permissions { get; set; } = new();
    [JsonProperty("comment")] public string? Comment { get; set; }
}

public sealed class Relation
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("types")] public List<RelationType> Types { get; set; } = new();
    [JsonProperty("comment")] public string? Comment { get; set; }
}

public sealed class RelationType
{
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("relation")] public string? Relation { get; set; }
    [JsonProperty("comment")] public string? Comment { get; set; }
}

public sealed class Permission
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("userSet")] public UserSet UserSet { get; set; }
    [JsonProperty("comment")] public string? Comment { get; set; }
}

public sealed class UserSet
{
    [JsonProperty("operation")] public string? Operation { get; set; }
    [JsonProperty("relation")] public string? Relation { get; set; }
    [JsonProperty("permission")] public string? Permission { get; set; }
    [JsonProperty("children")] public List<UserSet> Children { get; set; } = new();
}
