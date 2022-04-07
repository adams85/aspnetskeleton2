using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace POTools.Services.Extracting;

public static class SyntaxNodeExtensions
{
    public static int GetLineNumber(this SyntaxNode node, CancellationToken cancellationToken)
    {
        var lineSpan = node.SyntaxTree.GetMappedLineSpan(node.Span, cancellationToken);
        return lineSpan.StartLinePosition.Line + 1;
    }

    public static string? ResolveStringConstantExpression(this ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal && literal.Token.IsKind(SyntaxKind.StringLiteralToken))
            return literal.Token.ValueText;

        var values = new List<string>();

        foreach (var node in expression.DescendantNodesAndSelf())
            switch (node)
            {
                case BinaryExpressionSyntax binaryExpression:
                    if (!binaryExpression.IsKind(SyntaxKind.AddExpression))
                        return null;
                    break;
                case LiteralExpressionSyntax literalExpression:
                    if (!literalExpression.Token.IsKind(SyntaxKind.StringLiteralToken))
                        return null;
                    values.Add(literalExpression.Token.ValueText);
                    break;
                case ParenthesizedExpressionSyntax _:
                    break;
                default:
                    return null;
            }

        return string.Concat(values);
    }

    private static string GetTypeName(SyntaxToken identifier, int arity) =>
        arity > 0 ? identifier.ValueText + '<' + new string(',', arity - 1) + '>' : identifier.ValueText;

    public static string GetTypeName(this BaseTypeDeclarationSyntax declaration) =>
        GetTypeName(declaration.Identifier, declaration is TypeDeclarationSyntax typeDeclaration ? typeDeclaration.Arity : 0);

    public static string GetTypeName(this DelegateDeclarationSyntax declaration) =>
        GetTypeName(declaration.Identifier, declaration.Arity);

    public static IEnumerable<string> GetContainerNames(this MemberDeclarationSyntax declaration, bool includeNamespace = false)
    {
        using (var enumerator = declaration.Ancestors().GetEnumerator())
        {
            NamespaceDeclarationSyntax? namespaceDeclaration;

            for (; ; )
            {
                if (!enumerator.MoveNext())
                    yield break;

                if (enumerator.Current is TypeDeclarationSyntax typeDeclaration)
                    yield return GetTypeName(typeDeclaration);
                else if ((namespaceDeclaration = enumerator.Current as NamespaceDeclarationSyntax) != null)
                    break;
            }

            if (includeNamespace)
            {
                yield return namespaceDeclaration.Name.ToString();

                while (enumerator.MoveNext())
                    if ((namespaceDeclaration = enumerator.Current as NamespaceDeclarationSyntax) != null)
                        yield return namespaceDeclaration.Name.ToString();
            }
        }
    }
}
