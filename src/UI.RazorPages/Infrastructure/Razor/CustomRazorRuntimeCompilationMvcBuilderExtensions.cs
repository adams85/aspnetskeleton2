using System;
using System.Diagnostics;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.Extensions.DependencyInjection;

// TODO: remove this workaround when https://github.com/dotnet/aspnetcore/issues/40866 gets resolved
public static class RazorRuntimeCompilationMvcBuilderExtensions
{
    public static IMvcBuilder AddRazorRuntimeCompilation(this IMvcBuilder builder, bool suppressExplicitNullableWarning)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder.AddRazorRuntimeCompilation(suppressExplicitNullableWarning, CachedDelegates.Noop<MvcRazorRuntimeCompilationOptions>.Action);
    }

    public static IMvcBuilder AddRazorRuntimeCompilation(this IMvcBuilder builder, bool suppressExplicitNullableWarning, Action<MvcRazorRuntimeCompilationOptions> setupAction)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (setupAction == null)
            throw new ArgumentNullException(nameof(setupAction));

        builder.AddRazorRuntimeCompilation(setupAction);

        if (suppressExplicitNullableWarning)
        {
            var index = builder.Services.FindLastIndex(service => service.ServiceType == typeof(RazorProjectEngine));
            var engineFactory = index >= 0 ? builder.Services[index].ImplementationFactory as Func<IServiceProvider, RazorProjectEngine> : null;
            Debug.Assert(engineFactory != null, "Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation internals have apparently changed.");

            builder.Services[index] = ServiceDescriptor.Singleton(sp =>
            {
                var engine = engineFactory(sp);

                // creates a clone of the original engine instance configured by the framework and adds our customization to suppress the warning
                return RazorProjectEngine.Create(engine.Configuration, engine.FileSystem, builder =>
                {
                    builder.Features.Clear();
                    for (int i = 0, n = engine.EngineFeatures.Count; i < n; i++)
                        builder.Features.Add(engine.EngineFeatures[i]);

                    for (int i = 0, n = engine.ProjectFeatures.Count; i < n; i++)
                        builder.Features.Add(engine.ProjectFeatures[i]);

                    builder.Features.Add(new SuppressExplicitNullableWarningPass());

                    builder.Phases.Clear();
                    for (int i = 0, n = engine.Phases.Count; i < n; i++)
                        builder.Phases.Add(engine.Phases[i]);
                });
            });
        }

        return builder;
    }

    private sealed class SuppressExplicitNullableWarningPass : IntermediateNodePassBase, IRazorOptimizationPass
    {
        // Run late in the optimization phase
        public override int Order => int.MaxValue;

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            documentNode.Children.Insert(0, new CSharpCodeIntermediateNode
            {
                Children =
                {
                    new IntermediateToken()
                    {
                        Content = "#pragma warning disable 8669" + Environment.NewLine,
                        Kind = TokenKind.CSharp,
                    }
                }
            });
        }
    }
}
