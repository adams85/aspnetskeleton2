using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WebApp.Service;
using WebApp.Service.Helpers;
using WebApp.UI.Helpers;

namespace WebApp.UI.Areas.Dashboard.Models.DataTables
{
    public sealed class DataTableModel
    {
        public DataTableModel(IListQuery query, IListResult result, Type itemType)
        {
            Query = query;
            Result = result;
            ItemType = itemType;

            GetReturnUrlRouteValues = GetReturnUrlRouteValuesDefault;
        }

        public DataTableModel(IDataTableSource source) : this(source.Query, source.Result, source.ItemType) { }

        public IListQuery Query { get; }

        private (string, bool)[]? _queryOrderByElements;
        public (string KeyPropertyPath, bool Descending)[] QueryOrderByElements =>
            _queryOrderByElements ??= Query.OrderBy?.Select(QueryableHelper.ParseOrderByElement).ToArray() ?? Array.Empty<(string, bool)>();

        public IListResult Result { get; }

        public Type ItemType { get; }

        private string _tableHtmlId = "data-table";
        public string TableHtmlId
        {
            get => _tableHtmlId;
            set => (_tableHtmlId, _filterFormHtmlId) = (value, null);
        }

        private string? _filterFormHtmlId;
        public string FilterFormHtmlId => _filterFormHtmlId ??= _tableHtmlId + "-filter-form";

        public bool AllowSorting { get; set; } = true;
        public bool AllowFiltering { get; set; } = true;
        public bool AllowPaging { get; set; } = true;
        public bool AllowEditing { get; set; } = true;

        public bool CanCreateRow { get; set; } = true;

        public Func<object, object?> GetRowId { get; set; } = item => null;

        private Func<IUrlHelper, object?, string> _generateDisplayUrl = (url, values) => url.Page("./Index", values);
        public Func<IUrlHelper, object?, string> GenerateDisplayUrl
        {
            get => _generateDisplayUrl;
            set => (_generateDisplayUrl, _returnUrlRouteValues) = (value, null);
        }

        public Func<IUrlHelper, object?, string> GenerateCreateUrl { get; set; } = (url, values) => url.Page("./Create", values);
        public Func<IUrlHelper, object?, string> GenerateEditUrl { get; set; } = (url, values) => url.Page("./Edit", values);
        public Func<IUrlHelper, object?, string> GenerateDeleteUrl { get; set; } = (url, values) => url.Page("./Delete", values);

        private object? _returnUrlRouteValues;
        public Func<IUrlHelper, object> GetReturnUrlRouteValues { get; set; }

        private object GetReturnUrlRouteValuesDefault(IUrlHelper url)
        {
            if (_returnUrlRouteValues == null)
            {
                var routeValues = new RouteValueDictionary();
                routeValues.Merge(url.ActionContext.HttpContext.Request.Query);
                _returnUrlRouteValues = new { ReturnUrl = GenerateDisplayUrl(url, routeValues) };
            }

            return _returnUrlRouteValues;
        }

        public string? OrderByFormFieldName { get; set; }
        public string? PageIndexFormFieldName { get; set; }
        public string? PageSizeFormFieldName { get; set; }

        public PagerRenderOptions? PagerRenderOptions { get; set; }

        public Func<DataTableModel, IHtmlContent>? TableTemplate { get; set; }

        public bool ShowTableHeaderRow { get; set; } = true;
        public Func<DataTableModel, IHtmlContent>? TableHeaderRowTemplate { get; set; }

        public bool ShowColumnHeaderRow { get; set; } = true;
        public Func<DataTableModel, IHtmlContent>? ColumnHeaderRowTemplate { get; set; }

        public Func<DataTableModel, IHtmlContent>? ColumnFilterRowTemplate { get; set; }

        public Func<(object Item, DataTableModel TableModel), IHtmlContent>? ContentRowTemplate { get; set; }

        public Func<DataTableModel, IHtmlContent>? NoContentRowTemplate { get; set; }

        public bool ShowTableFooterRow { get; set; } = true;
        public Func<DataTableModel, IHtmlContent>? TableFooterRowTemplate { get; set; }

        private List<DataTableColumnModel>? _columns;
        public List<DataTableColumnModel> Columns
        {
            get => _columns ??= new List<DataTableColumnModel>();
            set => _columns = value;
        }

        public Func<DataTableModel, IEnumerable<DataTableColumnModel>> ColumnsFactory
        {
            set => Columns = new List<DataTableColumnModel>(value(this));
        }
    }
}
