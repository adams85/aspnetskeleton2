using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.UI.Helpers.Razor
{
    public interface ITableHelpers
    {
        void ColumnHeader(string name, string columnName, (string ColumnName, bool Descending)[] currentOrderColumns, LocalizedHtmlString title,
            string ascendingIconClassName, string descendingIconClassName);
    }
}
