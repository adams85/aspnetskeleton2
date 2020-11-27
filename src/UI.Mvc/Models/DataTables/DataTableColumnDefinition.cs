using System;
using Microsoft.AspNetCore.Html;

namespace WebApp.UI.Models.DataTables
{
    public abstract class DataTableColumnDefinition
    {
        protected DataTableColumnDefinition(DataTableDefinition table)
        {
            Table = table;
        }

        public DataTableDefinition Table { get; }

        public string? Title { get; set; }

        public bool IsSortable { get; set; } = true;
        public string? OrderKeyPropertyPath { get; set; }
        public string? AscendingOrderIconCssClass { get; set; }
        public string? DescendingOrderIconCssClass { get; set; }
        public Func<DataTableColumnDefinition, IHtmlContent>? HeaderTemplate { get; set; }

        public bool IsFilterable { get; set; } = true;

        public DataTableColumnFilterDefinition? Filter { get; set; }

        public Func<DataTableColumnDefinition, DataTableColumnFilterDefinition> FilterFactory
        {
            set => Filter = value(this);
        }

        public Func<DataTableColumnDefinition, IHtmlContent>? FilterTemplate { get; set; }

        public Func<(object Item, DataTableColumnDefinition ColumnDefinition), IHtmlContent>? DataCellTemplate { get; set; }

        public sealed class DataColumn : DataTableColumnDefinition
        {
            public DataColumn(DataTableDefinition table, DataTableColumnBinding binding) : base(table)
            {
                if (table.ItemType != binding.ItemType)
                    throw new ArgumentException("Item type of table definition and column binding mismatch.", nameof(table));

                Binding = binding;
            }

            public DataTableColumnBinding Binding { get; }
        }
    }
}
