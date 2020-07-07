using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace POTools.Services.Extracting
{
    public class CSharpTextExtractor : ILocalizableTextExtractor
    {
        private readonly string _localizedAttributeName;
        private readonly string _localizedAttributeFullName;
        private readonly string _localizedAttributePluralIdArgName;
        private readonly string _localizedAttributeContextIdArgName;

        private readonly string _localizerMemberName;

        private readonly string _pluralTypeName;
        private readonly string _pluralFactoryMemberName;

        private readonly string _textContextTypeName;
        private readonly string _textContextFactoryMemberName;

        public CSharpTextExtractor() : this(null) { }

        public CSharpTextExtractor(CSharpTextExtractorSettings? settings)
        {
            _localizedAttributeName = settings?.LocalizedAttributeName ?? CSharpTextExtractorSettings.DefaultLocalizedAttributeName;
            _localizedAttributeFullName = _localizedAttributeName + "Attribute";
            _localizedAttributePluralIdArgName = settings?.LocalizedAttributePluralIdArgName ?? CSharpTextExtractorSettings.DefaultLocalizedAttributePluralIdArgName;
            _localizedAttributeContextIdArgName = settings?.LocalizedAttributeContextIdArgName ?? CSharpTextExtractorSettings.DefaultLocalizedAttributeContextIdArgName;

            _localizerMemberName = settings?.LocalizerMemberName ?? CSharpTextExtractorSettings.DefaultLocalizerMemberName;

            _pluralTypeName = settings?.PluralTypeName ?? CSharpTextExtractorSettings.DefaultPluralTypeName;
            _pluralFactoryMemberName = settings?.PluralFactoryMemberName ?? CSharpTextExtractorSettings.DefaultPluralFactoryMemberName;

            _textContextTypeName = settings?.TextContextTypeName ?? CSharpTextExtractorSettings.DefaultTextContextTypeName;
            _textContextFactoryMemberName = settings?.TextContextFactoryMemberName ?? CSharpTextExtractorSettings.DefaultTextContextFactoryMemberName;
        }

        protected virtual string GetCode(string content, CancellationToken cancellationToken)
        {
            return content;
        }

        protected virtual SyntaxTree ParseText(string code, CancellationToken cancellationToken)
        {
            return CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GetRelevantDeclarations(SyntaxTree syntaxTree, CancellationToken cancellationToken)
        {
            return syntaxTree.GetRoot(cancellationToken).DescendantNodes()
                .OfType<MemberDeclarationSyntax>()
                .Where(node =>
                    node is EnumMemberDeclarationSyntax ||
                    node is BaseFieldDeclarationSyntax ||
                    node is BasePropertyDeclarationSyntax ||
                    node is BaseMethodDeclarationSyntax);
        }

        private IEnumerable<LocalizableTextInfo> AnalyzeDecoratedDeclaration(MemberDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            AttributeSyntax? attribute;
            IEnumerable<string> ids;

            if (declaration is FieldDeclarationSyntax fieldDeclaration &&
                fieldDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ConstKeyword)) &&
                (attribute = GetAttribute(declaration.AttributeLists)) != null)
            {
                ids = fieldDeclaration.Declaration.Variables
                    .Select(declarator => declarator.Initializer?.Value.ResolveStringConstantExpression())
                    .Where(value => value != null)!;
            }
            else if (declaration is EnumMemberDeclarationSyntax enumMemberDeclaration &&
                (attribute = GetAttribute(declaration.AttributeLists)) != null)
            {
                var enumDeclaration = declaration.Ancestors().OfType<EnumDeclarationSyntax>().First();

                var id = string.Join('.', enumDeclaration.GetContainerNames(includeNamespace: true).Reverse()
                    .Append(enumDeclaration.GetTypeName())
                    .Append(enumMemberDeclaration.Identifier.ValueText));

                ids = new[] { id };
            }
            else
                return Enumerable.Empty<LocalizableTextInfo>();

            var lineNumber = declaration.GetLineNumber(cancellationToken);
            var pluralId = GetAttributeParamValue(attribute, _localizedAttributePluralIdArgName);
            var contextId = GetAttributeParamValue(attribute, _localizedAttributeContextIdArgName);

            return ids.Select(id => new LocalizableTextInfo
            {
                LineNumber = lineNumber,
                Id = id,
                PluralId = pluralId,
                ContextId = contextId,
            });

            AttributeSyntax? GetAttribute(SyntaxList<AttributeListSyntax> attributeLists)
            {
                return attributeLists
                    .SelectMany(attributeList => attributeList.Attributes)
                    .FirstOrDefault(attribute =>
                        // only simple (unqualified) type names are supported because accepting qualified type names would be too much hassle for little gain
                        attribute.Name is IdentifierNameSyntax attributeName &&
                        (attributeName.Identifier.ValueText == _localizedAttributeName || attributeName.Identifier.ValueText == _localizedAttributeFullName));
            }

            string? GetAttributeParamValue(AttributeSyntax attribute, string argName)
            {
                var arg = attribute.ArgumentList?.Arguments
                    .Where(arg => arg.NameEquals?.Name.Identifier.ValueText == argName)
                    .FirstOrDefault();

                return arg?.Expression.ResolveStringConstantExpression();
            }
        }

        private IEnumerable<LocalizableTextInfo> AnalyzeElementAccessExpressions(MemberDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            return declaration.DescendantNodes()
                .OfType<ElementAccessExpressionSyntax>()
                .Where(elementAccess => elementAccess.Expression is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == _localizerMemberName)
                .Select(elementAccess => GetTextInfo(elementAccess, cancellationToken));

            LocalizableTextInfo GetTextInfo(ElementAccessExpressionSyntax translateExpression, CancellationToken cancellationToken)
            {
                var lineNumber = translateExpression.GetLineNumber(cancellationToken);

                var argList = translateExpression.ArgumentList;
                var id = GetId(argList);
                if (id == null)
                    return new LocalizableTextInfo { LineNumber = lineNumber };

                return new LocalizableTextInfo
                {
                    LineNumber = lineNumber,
                    Id = id,
                    PluralId = GetPluralId(argList),
                    ContextId = GetContextId(argList),
                };
            }

            static string? GetId(BaseArgumentListSyntax node)
            {
                var args = node.Arguments;
                return
                    args.Count > 0 &&
                        args[0] is ArgumentSyntax arg &&
                        arg.Expression is LiteralExpressionSyntax literal &&
                        literal.Token.IsKind(SyntaxKind.StringLiteralToken) ?
                    literal.Token.ValueText :
                    null;
            }

            string? GetPluralId(BaseArgumentListSyntax node)
            {
                var factoryInvocation = node.Arguments
                    .Skip(1)
                    .Select(arg =>
                        arg.Expression is InvocationExpressionSyntax invocation &&
                            invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                            memberAccess.Expression is IdentifierNameSyntax typeName &&
                            typeName.Identifier.ValueText == _pluralTypeName &&
                            memberAccess.Name is IdentifierNameSyntax memberName &&
                            memberName.Identifier.ValueText == _pluralFactoryMemberName ?
                        invocation :
                        null)
                    .FirstOrDefault(invocation => invocation != null);

                if (factoryInvocation == null)
                    return null;

                var args = factoryInvocation.ArgumentList.Arguments;
                return
                    args.Count == 2 &&
                        args[0] is ArgumentSyntax argument &&
                        argument.Expression is LiteralExpressionSyntax literal &&
                        literal.Token.IsKind(SyntaxKind.StringLiteralToken) ?
                    literal.Token.ValueText :
                    null;
            }

            string? GetContextId(BaseArgumentListSyntax node)
            {
                var args = node.Arguments;
                if (!(args.Count > 1 && args[^1] is ArgumentSyntax arg &&
                    arg.Expression is InvocationExpressionSyntax factoryInvocation &&
                        factoryInvocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression is IdentifierNameSyntax typeName &&
                        typeName.Identifier.ValueText == _textContextTypeName &&
                        memberAccess.Name is IdentifierNameSyntax memberName &&
                        memberName.Identifier.ValueText == _textContextFactoryMemberName))
                    return null;

                args = factoryInvocation.ArgumentList.Arguments;
                return
                    args.Count == 1 &&
                        args[0] is ArgumentSyntax argument &&
                        argument.Expression is LiteralExpressionSyntax literal &&
                        literal.Token.IsKind(SyntaxKind.StringLiteralToken) ?
                    literal.Token.ValueText :
                    null;
            }
        }

        private IEnumerable<LocalizableTextInfo> ScanDeclaration(MemberDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            return declaration switch
            {
                EnumMemberDeclarationSyntax _ => AnalyzeDecoratedDeclaration(declaration, cancellationToken),
                BaseFieldDeclarationSyntax _ => AnalyzeDecoratedDeclaration(declaration, cancellationToken)
                    .Concat(AnalyzeElementAccessExpressions(declaration, cancellationToken)),
                BasePropertyDeclarationSyntax _ => AnalyzeElementAccessExpressions(declaration, cancellationToken),
                BaseMethodDeclarationSyntax _ => AnalyzeElementAccessExpressions(declaration, cancellationToken),
                _ => Enumerable.Empty<LocalizableTextInfo>()
            };
        }

        public IEnumerable<LocalizableTextInfo> Extract(string content, CancellationToken cancellationToken = default)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var code = GetCode(content, cancellationToken);

            var syntaxTree = ParseText(code, cancellationToken);
            if (syntaxTree.GetDiagnostics(cancellationToken).Any(d => d.Severity >= DiagnosticSeverity.Error))
                throw new ArgumentException("Source code has errors", nameof(content));

            var root = syntaxTree.GetRoot(cancellationToken);

            return GetRelevantDeclarations(syntaxTree, cancellationToken)
                .SelectMany(declaration => ScanDeclaration(declaration, cancellationToken));
        }
    }
}
