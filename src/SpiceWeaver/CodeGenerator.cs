using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var schema = JsonSerializer.Deserialize<Schema>(schemaJson);

            Debug.Assert(schema is not null);

            return Generate(@namespace, className, schema!);
        }
        catch (JsonException e) { throw new SpiceWeaverException("Error deserializing schema from json", e); }
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
        var nameSpace = ConstNullableStringField("Namespace", definition.Namespace);
        var withIdMethod = WithIdMethod(definition.Name, definition.Namespace);

        var relations = definition.Relations.Select(RelationField).ToArray();
        var permissions = definition.Permissions.Select(PermissionField).ToArray();

        var definitionClass = StaticClass(definition.Name.ToPascalCase())
            .AddMembers(name, nameSpace, withIdMethod);

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

    private static MemberDeclarationSyntax ConstNullableStringField(string name, string? value) =>
        FieldDeclaration(
                VariableDeclaration(NullableType(StringType))
                    .AddVariables(
                        VariableDeclarator(Identifier(name))
                            .WithInitializer(
                                EqualsValueClause(
                                    value is null
                                        ? LiteralExpression(SyntaxKind.NullLiteralExpression)
                                        : LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value))
                                )
                            )
                    )
            )
            .AddModifiers(Public, Const);

    private static VariableDeclarationSyntax InitializedStringVariable(string name, string value) =>
        VariableDeclaration(StringType)
            .AddVariables(
                VariableDeclarator(Identifier(name))
                    .WithInitializer(
                        EqualsValueClause(
                            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value))
                        )));

    private static MethodDeclarationSyntax WithIdMethod(string resourceName, string? @namespace)
    {
        const string idParameterIdentifier = "id";
        const string includeNamespaceParameterIdentifier = "includeNamespace";

        ExpressionSyntax expressionBody = string.IsNullOrWhiteSpace(@namespace)
            ? ResourceIdInterpolationExpression(resourceName, idParameterIdentifier)
            : ConditionallyNamespacedResourceIdExpression(resourceName, @namespace!, idParameterIdentifier,
                includeNamespaceParameterIdentifier);

        return MethodDeclaration(StringType, Identifier("WithId"))
            .AddModifiers(Public, Static)
            .AddParameterListParameters(StringParameter(idParameterIdentifier))
            .AddParameterListParameters(BoolParameter(includeNamespaceParameterIdentifier, false))
            .WithExpressionBody(
                ArrowExpressionClause(expressionBody)
            )
            .WithSemicolonToken(SemiColon);

        static InterpolatedStringExpressionSyntax ResourceIdInterpolationExpression(string resourceName,
            string idIdentifier) =>
            InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken))
                .AddContents(
                    InterpolatedStringText(
                        Token(
                            TriviaList(), SyntaxKind.InterpolatedStringTextToken, $"{resourceName}:", "", TriviaList()
                        )
                    ), Interpolation(IdentifierName(idIdentifier))
                );

        static ConditionalExpressionSyntax ConditionallyNamespacedResourceIdExpression(string resourceName,
            string @namespace,
            string idIdentifier, string includeNamespaceIdentifier) => ConditionalExpression(
            IdentifierName(includeNamespaceIdentifier),
            InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken))
                .AddContents(
                    InterpolatedStringText(
                        Token(
                            TriviaList(), SyntaxKind.InterpolatedStringTextToken, $"{@namespace}/{resourceName}:", "",
                            TriviaList()
                        )
                    ),
                    Interpolation(IdentifierName(idIdentifier))
                ),
            InterpolatedStringExpression(
                    Token(SyntaxKind.InterpolatedStringStartToken))
                .AddContents(
                    InterpolatedStringText(
                        Token(
                            TriviaList(), SyntaxKind.InterpolatedStringTextToken, $"{resourceName}:", "",
                            TriviaList())
                    ),
                    Interpolation(IdentifierName(idIdentifier))
                )
        );
    }

    private static ParameterSyntax StringParameter(string identifier) =>
        Parameter(Identifier(identifier)).WithType(StringType);

    private static ParameterSyntax BoolParameter(string identifier, bool? defaultValue)
    {
        var parameter = Parameter(Identifier(identifier)).WithType(PredefinedType(Token(SyntaxKind.BoolKeyword)));

        if (defaultValue is not null)
        {
            parameter = parameter.WithDefault(
                EqualsValueClause(
                    LiteralExpression(
                        defaultValue.Value
                            ? SyntaxKind.TrueLiteralExpression
                            : SyntaxKind.FalseLiteralExpression
                    )
                )
            );
        }

        return parameter;
    }

    private static PredefinedTypeSyntax StringType => PredefinedType(Token(SyntaxKind.StringKeyword));

    private static SyntaxToken Const => Token(SyntaxKind.ConstKeyword);
    private static SyntaxToken Public => Token(SyntaxKind.PublicKeyword);
    private static SyntaxToken Static => Token(SyntaxKind.StaticKeyword);
    private static SyntaxToken SemiColon => Token(SyntaxKind.SemicolonToken);
}