using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.UI.Models.DataTables
{
    public abstract class DataTableColumnFilter
    {
        public string? FormFieldName { get; set; }
        public object? FormFieldValue { get; set; }

        public sealed class Text : DataTableColumnFilter
        {
            public LocalizedHtmlString? PlaceholderText { get; set; }
        }
    }
}
