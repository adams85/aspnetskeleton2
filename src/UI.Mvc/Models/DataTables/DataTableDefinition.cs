using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using WebApp.Service;
using WebApp.Service.Helpers;

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

        private (string, bool)[]? _queryOrderingComponents;
        public (string KeyPath, bool Descending)[] QueryOrderingComponents =>
            _queryOrderingComponents ??= Query.OrderBy?.Select(QueryableHelper.ParseOrderingComponent).ToArray() ?? Array.Empty<(string, bool)>();

        public ListResult<TItem> Result { get; }

        public Func<IUrlHelper, object?, string> GenerateQueryUrl { get; set; } = (url, values) => url.Action(null, values);

        public Func<DataTableDefinition<TItem>, IHtmlContent>? TableTemplate { get; set; }

        public bool ShowTableHeaderRow { get; set; } = true;
        public Func<DataTableDefinition<TItem>, IHtmlContent>? HeaderRowTemplate { get; set; }

        public bool ShowColumnHeaderRow { get; set; } = true;
        public string? OrderByFormFieldName { get; set; }
        public Func<DataTableDefinition<TItem>, IHtmlContent>? ColumnHeaderRowTemplate { get; set; }

        public bool ShowColumnFilterRow { get; set; } = true;
        public Func<DataTableDefinition<TItem>, IHtmlContent>? ColumnFilterRowTemplate { get; set; }

        public Func<(TItem Data, DataTableDefinition<TItem> TableDefinition), IHtmlContent>? DataRowTemplate { get; set; }
        public Func<DataTableDefinition<TItem>, IHtmlContent>? NoDataRowTemplate { get; set; }

        private List<DataTableColumnDefinition<TItem>>? _columns;
        public List<DataTableColumnDefinition<TItem>> Columns
        {
            get => _columns ??= new List<DataTableColumnDefinition<TItem>>();
            set => _columns = value;
        }
    }
}
