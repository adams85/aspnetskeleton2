using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using WebApp.Service;
using WebApp.Service.Helpers;
using WebApp.UI.Helpers;

namespace WebApp.UI.Models.DataTables
{
    public sealed class DataTableDefinition
    {
        public DataTableDefinition(IListQuery query, IListResult result, Type itemType)
        {
            Query = query;
            Result = result;
            ItemType = itemType;
        }

        public DataTableDefinition(IDataTableSource source) : this(source.Query, source.Result, source.ItemType) { }

        public IListQuery Query { get; }

        private (string, bool)[]? _queryOrderByElements;
        public (string KeyPropertyPath, bool Descending)[] QueryOrderByElements =>
            _queryOrderByElements ??= Query.OrderBy?.Select(QueryableHelper.ParseOrderByElement).ToArray() ?? Array.Empty<(string, bool)>();

        public IListResult Result { get; }

        public Type ItemType { get; }

        public Func<IUrlHelper, object?, string> GenerateQueryUrl { get; set; } = (url, values) => url.Action(null, values);

        public bool AllowSorting { get; set; } = true;
        public bool AllowFiltering { get; set; } = true;
        public bool AllowPaging { get; set; } = true;

        public string? OrderByFormFieldName { get; set; }
        public string? PageIndexFormFieldName { get; set; }
        public string? PageSizeFormFieldName { get; set; }

        public PagerRenderOptions? PagerRenderOptions { get; set; }

        public Func<DataTableDefinition, IHtmlContent>? TableTemplate { get; set; }

        public bool ShowTableHeaderRow { get; set; } = true;
        public Func<DataTableDefinition, IHtmlContent>? TableHeaderRowTemplate { get; set; }

        public bool ShowColumnHeaderRow { get; set; } = true;
        public Func<DataTableDefinition, IHtmlContent>? ColumnHeaderRowTemplate { get; set; }

        public Func<DataTableDefinition, IHtmlContent>? ColumnFilterRowTemplate { get; set; }

        public Func<(object Item, DataTableDefinition TableDefinition), IHtmlContent>? DataRowTemplate { get; set; }

        public Func<DataTableDefinition, IHtmlContent>? NoDataRowTemplate { get; set; }

        public bool ShowTableFooterRow { get; set; } = true;
        public Func<DataTableDefinition, IHtmlContent>? TableFooterRowTemplate { get; set; }

        private List<DataTableColumnDefinition>? _columns;
        public List<DataTableColumnDefinition> Columns
        {
            get => _columns ??= new List<DataTableColumnDefinition>();
            set => _columns = value;
        }

        public Func<DataTableDefinition, IEnumerable<DataTableColumnDefinition>> ColumnsFactory
        {
            set => Columns = new List<DataTableColumnDefinition>(value(this));
        }
    }
}
