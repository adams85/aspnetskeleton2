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
    public class DataTableDefinition<TItem>
    {
        public DataTableDefinition(ListQuery query, ListResult<TItem> result)
        {
            Query = query;
            Result = result;
        }

        public ListQuery Query { get; }

        private (string, bool)[]? _queryOrderByElements;
        public (string KeyPropertyPath, bool Descending)[] QueryOrderByElements =>
            _queryOrderByElements ??= Query.OrderBy?.Select(QueryableHelper.ParseOrderByElement).ToArray() ?? Array.Empty<(string, bool)>();

        public ListResult<TItem> Result { get; }

        public Func<IUrlHelper, object?, string> GenerateQueryUrl { get; set; } = (url, values) => url.Action(null, values);

        public bool AllowSorting { get; set; } = true;
        public bool AllowFiltering { get; set; } = true;
        public bool AllowPaging { get; set; } = true;

        public string? OrderByFormFieldName { get; set; }
        public string? PageIndexFormFieldName { get; set; }
        public string? PageSizeFormFieldName { get; set; }

        public PagerRenderOptions? PagerRenderOptions { get; set; }

        public Func<DataTableDefinition<TItem>, IHtmlContent>? TableTemplate { get; set; }

        public bool ShowTableHeaderRow { get; set; } = true;
        public Func<DataTableDefinition<TItem>, IHtmlContent>? TableHeaderRowTemplate { get; set; }

        public bool ShowColumnHeaderRow { get; set; } = true;
        public Func<DataTableDefinition<TItem>, IHtmlContent>? ColumnHeaderRowTemplate { get; set; }

        public Func<DataTableDefinition<TItem>, IHtmlContent>? ColumnFilterRowTemplate { get; set; }

        public Func<(TItem Data, DataTableDefinition<TItem> TableDefinition), IHtmlContent>? DataRowTemplate { get; set; }

        public Func<DataTableDefinition<TItem>, IHtmlContent>? NoDataRowTemplate { get; set; }

        public bool ShowTableFooterRow { get; set; } = true;
        public Func<DataTableDefinition<TItem>, IHtmlContent>? TableFooterRowTemplate { get; set; }

        private List<DataTableColumnDefinition<TItem>>? _columns;
        public List<DataTableColumnDefinition<TItem>> Columns
        {
            get => _columns ??= new List<DataTableColumnDefinition<TItem>>();
            set => _columns = value;
        }
    }
}
