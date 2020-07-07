using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace POTools.Services.Extracting
{
    public class CSharpRazorTextExtractor : CSharpTextExtractor
    {
        private readonly RazorProjectEngine _projectEngine;

        public CSharpRazorTextExtractor() : this(null) { }

        public CSharpRazorTextExtractor(CSharpTextExtractorSettings? settings) : base(settings)
        {
            _projectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, RazorProjectFileSystem.Create(@"\"));
        }

        protected override string GetCode(string content, CancellationToken cancellationToken)
        {
            var sourceDocument = RazorSourceDocument.Create(content, "_");
            var codeDocument = _projectEngine.Process(sourceDocument, fileKind: null, importSources: null, tagHelpers: null);
            var parsedDocument = codeDocument.GetCSharpDocument();
            if (parsedDocument.Diagnostics.OfType<RazorDiagnostic>().Any(d => d.Severity == RazorDiagnosticSeverity.Error))
                throw new ArgumentException("Razor code has errors.", nameof(content));

            return parsedDocument.GeneratedCode;
        }

        protected override IEnumerable<MemberDeclarationSyntax> GetRelevantDeclarations(SyntaxTree syntaxTree, CancellationToken cancellationToken)
        {
            return syntaxTree.GetRoot(cancellationToken).DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(md => !md.Modifiers.Any(sk => sk.IsKind(SyntaxKind.StaticKeyword)));
        }
    }
}
