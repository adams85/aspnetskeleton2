﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 17.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace CodegenTools.Templates
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class QueryHandler : QueryTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("﻿");
            this.Write("using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing Syste" +
                    "m.Threading;\r\nusing System.Threading.Tasks;\r\nusing Microsoft.EntityFrameworkCore" +
                    ";\r\nusing ");
            
            #line 12 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Namespace}.Core.Helpers"));
            
            #line default
            #line hidden
            this.Write(";\r\nusing ");
            
            #line 13 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Namespace}.DataAccess.Entities"));
            
            #line default
            #line hidden
            this.Write(";\r\n\r\nnamespace ");
            
            #line 15 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Namespace}.Service.{Group}"));
            
            #line default
            #line hidden
            this.Write(";\r\n\r\n");
            
            #line 17 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"

var resultType = GetResultType();

            
            #line default
            #line hidden
            this.Write("internal sealed class ");
            
            #line 20 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Name}QueryHandler"));
            
            #line default
            #line hidden
            this.Write(" : ");
            
            #line 20 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetQueryHandlerBaseType(resultType)));
            
            #line default
            #line hidden
            this.Write("\r\n{\r\n    public override async Task<");
            
            #line 22 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(resultType));
            
            #line default
            #line hidden
            this.Write("> HandleAsync(");
            
            #line 22 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture($"{Name}Query"));
            
            #line default
            #line hidden
            this.Write(" query, QueryContext context, CancellationToken cancellationToken)\r\n    {\r\n      " +
                    "  await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).Co" +
                    "nfigureAwait(false))\r\n        {\r\n");
            
            #line 26 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"

if (!IsList)
{

            
            #line default
            #line hidden
            this.Write("            Entity entity = await dbContext.Entities\r\n                .FindAsync(" +
                    "new object[] { query.Id }, cancellationToken).ConfigureAwait(false);\r\n\r\n        " +
                    "    RequireExisting(entity, q => q.Id);\r\n\r\n            // ...\r\n");
            
            #line 36 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"

    if (IsEventProducer)
    {

            
            #line default
            #line hidden
            this.Write("\r\n            query.OnEvent?.Invoke(query, new ProgressEvent\r\n            {\r\n    " +
                    "            Progress = 1f\r\n            });\r\n");
            
            #line 45 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"

    }

            
            #line default
            #line hidden
            this.Write("\r\n            return new ");
            
            #line 49 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(resultType));
            
            #line default
            #line hidden
            this.Write("\r\n            {\r\n            };\r\n");
            
            #line 52 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"

} 
else 
{

            
            #line default
            #line hidden
            this.Write("            IQueryable<Entity> entityQuery = dbContext.Entities;\r\n\r\n            /" +
                    "/ ...\r\n\r\n            var dataQuery = entityQuery.ToData();\r\n");
            
            #line 62 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"

    if (IsEventProducer)
    {

            
            #line default
            #line hidden
            this.Write(@"
            var items = await ToArrayAsync(ApplyPagingAndOrdering(query, dataQuery), cancellationToken).ConfigureAwait(false);

            var totalItemCount = await GetTotalItemCountAsync(query, dataQuery, items, cancellationToken).ConfigureAwait(false);

            query.OnEvent?.Invoke(query, new ProgressEvent
            {
                Progress = 1f
            });

            return Result(items, totalItemCount, query.PageIndex, query.PageSize, query.MaxPageSize);
");
            
            #line 77 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"

    }
    else
    {

            
            #line default
            #line hidden
            this.Write("\r\n            return await ResultAsync(query, dataQuery, cancellationToken).Confi" +
                    "gureAwait(false);\r\n");
            
            #line 84 "d:\Dev\_Templates\AspNetSkeleton\tools\CodegenTools\Templates\QueryHandler.tt"

    }
}

            
            #line default
            #line hidden
            this.Write("        }\r\n    }\r\n}\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
