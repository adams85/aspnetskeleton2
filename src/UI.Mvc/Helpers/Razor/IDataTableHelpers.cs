using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;

namespace WebApp.UI.Helpers.Razor
{
    public interface IDataTableHelpers
    {
        void TableHeaderRow(int columnCount);

        void ColumnHeaderRow(Func<object?, IHtmlContent> content);

        void ColumnHeader(string title);

        void ColumnHeader(string columnName, string formName, (string ColumnName, bool Descending)[] currentOrderColumns, RouteValueDictionary routeValues,
            string title, string ascendingIconClassName, string descendingIconClassName);

        void ColumnFilterRow(Func<object?, IHtmlContent> content);

        void TextColumnFilter(string columnName, string formName, string? currentValue, string title);

        void NoData(int columnCount);
    }
}
