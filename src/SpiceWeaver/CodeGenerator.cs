using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SpiceWeaver;

public static class CodeGenerator
{
    public static string Generate(string @namespace, string className, string schemaJson)
    {
        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            throw new ArgumentException("Cannot be null or whitespace", nameof(schemaJson));
        }

        try
        {
            var schema = JsonConvert.DeserializeObject<Schema>(schemaJson);
            
            Debug.Assert(schema is not null);

            return Generate(@namespace, className, schema!);
        }
        catch (JsonException e)
        {
            throw new SpiceWeaverException("Error deserializing schema from json", e);
        }
    }

    public static string Generate(string @namespace, string className, Schema schema)
    {
        using StringWriter stringWriter = new StringWriter();

        Generate(@namespace, className, schema, stringWriter);

        return stringWriter.ToString();
    }

    public static void Generate(string @namespace, string className, Schema schema, TextWriter writer)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            throw new ArgumentException("Cannot be null or whitespace", nameof(@namespace));
        }

        if (string.IsNullOrWhiteSpace(className))
        {
            throw new ArgumentException("Cannot be null or whitespace", nameof(className));
        }
        
        var syntax = GenerateSyntax(@namespace, className, schema);

        syntax.NormalizeWhitespace().WriteTo(writer);
    }

    public static NamespaceDeclarationSyntax GenerateSyntax(string @namespace, string className, Schema schema)
    {
        var definitionDeclarations =
            schema.Definitions.Select<Definition, MemberDeclarationSyntax>(CreateDefinition).ToArray();

        var definitionsClassDeclaration = StaticClass("Definitions").AddMembers(definitionDeclarations);

        var schemaClassDeclaration = StaticClass(className).AddMembers(definitionsClassDeclaration);

        return NamespaceDeclaration(ParseName(@namespace))
            .AddMembers(schemaClassDeclaration);
    }

    private static ClassDeclarationSyntax CreateDefinition(Definition definition)
    {
        var name = ConstStringField("Name", definition.Name);
        var relations = definition.Relations.Select(RelationField).ToArray();
        var permissions = definition.Permissions.Select(PermissionField).ToArray();

        var definitionClass = StaticClass(definition.Name.ToPascalCase())
            .AddMembers(name);

        if (relations.Any())
        {
            var relationsClass = StaticClass("Relations").AddMembers(relations);

            definitionClass = definitionClass.AddMembers(relationsClass);
        }

        if (permissions.Any())
        {
            var permissionsClass = StaticClass("Permissions").AddMembers(permissions);

            definitionClass = definitionClass.AddMembers(permissionsClass);
        }

        return definitionClass;
    }

    private static MemberDeclarationSyntax RelationField(Relation relation) =>
        ConstStringField(relation.Name.ToPascalCase(), relation.Name);

    private static MemberDeclarationSyntax PermissionField(Permission permission) =>
        ConstStringField(permission.Name.ToPascalCase(), permission.Name);

    private static ClassDeclarationSyntax StaticClass(string name) =>
        ClassDeclaration(Identifier(name))
            .AddModifiers(Public, Static);

    private static MemberDeclarationSyntax ConstStringField(string name, string value) =>
        FieldDeclaration(InitializedStringVariable(name, value))
            .AddModifiers(Public, Const);

    private static VariableDeclarationSyntax InitializedStringVariable(string name, string value) =>
        VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
            .AddVariables(
                VariableDeclarator(Identifier(name))
                    .WithInitializer(
                        EqualsValueClause(
                            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value))
                        )));

    private static SyntaxToken Const => Token(SyntaxKind.ConstKeyword);
    private static SyntaxToken Public => Token(SyntaxKind.PublicKeyword);
    private static SyntaxToken Static => Token(SyntaxKind.StaticKeyword);
}