# SpiceWeaver

SpiceWeaver is a .NET Source Generator for [SpiceDB](https://github.com/authzed/spicedb) schemas.

## Usage

Add SpiceWeaver to your `.csproj` file:

```xml
<ItemGroup>
    <PackageReference Include="SpiceWeaver" Version="0.2.0" PrivateAssets="all"/>
</ItemGroup>
```

### Generate from Schema File

To generate source directly from a schema file, **[spice2json](https://github.com/alsbury/spice2json)** must be
installed.

Add your SpiceDB schema to your `.csproj` file as an Additional File and flag it as a schema file:

```xml
<ItemGroup>
    <AdditionalFiles Include="schema.zed" SpiceWeaver_SchemaFile="true"/>
</ItemGroup>
```

### Generate from JSON file

If your schema has already been converted to a json file using **spice2json**, you can bypass conversion in the Source
Generator using the `IsJson`file option:

```xml
<ItemGroup>
    <AdditionalFiles Include="schema.json" SpiceWeaver_SchemaFile="true" SpiceWeaver_IsJson="true"/>
</ItemGroup>
```

### Output

Using the default settings, the generator will output a class named `SpiceWeaver.Schema`.

Given the following schema:

```
definition user {}

definition document {
    relation viewer: user
    relation editor: user

    permission view = viewer + editor
    permission edit = editor
}
```

The following source will be generated:

```csharp
namespace SpiceWeaver
{
    public static class Schema
    {
        public static class Definitions
        {
            public static class User
            {
                public const string Name = "user";
                public static string WithId(string id) => $"user:{id}";
            }

            public static class Document
            {
                public const string Name = "document";
                public static string WithId(string id) => $"document:{id}";
                public static class Relations
                {
                    public const string Viewer = "viewer";
                    public const string Editor = "editor";
                }

                public static class Permissions
                {
                    public const string View = "view";
                    public const string Edit = "edit";
                }
            }
        }
    }
}
```

## Options

All options are prefixed with `SpiceWeaver_`

### Global

| Name             | Description                                | Default      |
|------------------|--------------------------------------------|--------------|
| `Spice2JsonPath` | The relative path to the spice2json binary | `spice2json` |

Global options are set in the `.csproj` file directly in a `PropertyGroup` and the configured option applies to all
schema files

```xml
<PropertyGroup>
    <SpiceWeaver_Spice2JsonPath>path/to/spice2json</SpiceWeaver_Spice2JsonPath>
</PropertyGroup>
```

### File

| Name         | Description                                                                 | Default       |
|--------------|-----------------------------------------------------------------------------|---------------|
| `SchemaFile` | Set to `true` to indicate the file should be included for source generation | n/a           |
| `IsJson`     | Set to `true` to indicate the file is already a spice2json JSON file        | n/a           |
| `Namespace`  | Sets the namespace of the generated class                                   | `SpiceWeaver` |
| `ClassName`  | Sets the name of the generated class                                        | `Schema`      |

File options are per-file and are set on individual `AdditionalFiles` items

```xml
<ItemGroup>
    <AdditionalFiles Include="schema.zed" SpiceWeaver_SchemaFile="true" SpiceWeaver_Namespace="MyNamespace"/>
</ItemGroup>
```

