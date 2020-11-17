using System;
using Microsoft.AspNetCore.Html;
using WebApp.UI.Helpers.Models;

namespace WebApp.UI.Models.DataTables
{
    public class DataTableColumnDefinition<TItem>
    {
        public DataTableColumnDefinition(DataTableDefinition<TItem> table, ModelPropertyMetadata<TItem> property = default)
        {
            Table = table;
            Property = property;
        }

        public DataTableDefinition<TItem> Table { get; }
        public ModelPropertyMetadata<TItem> Property { get; }

        public string? Title { get; set; }

        public bool IsSortable { get; set; } = true;
        public string? OrderKeyPropertyPath { get; set; }
        public string? AscendingOrderIconCssClass { get; set; }
        public string? DescendingOrderIconCssClass { get; set; }
        public Func<DataTableColumnDefinition<TItem>, IHtmlContent>? HeaderTemplate { get; set; }

        public bool IsFilterable { get; set; } = true;
        public DataTableColumnFilter? Filter { get; set; }
        public Func<DataTableColumnDefinition<TItem>, IHtmlContent>? FilterTemplate { get; set; }

        public Func<(TItem Data, DataTableColumnDefinition<TItem> ColumnDefinition), IHtmlContent>? DataCellTemplate { get; set; }
    }
}
