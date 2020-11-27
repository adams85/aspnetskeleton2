using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.UI.Models.DataTables
{
    public abstract class DataTableColumnFilterDefinition
    {
        protected DataTableColumnFilterDefinition(DataTableColumnDefinition column)
        {
            Column = column;
        }

        protected DataTableColumnFilterDefinition(DataTableColumnDefinition column, string formFieldName, object? formFieldValue) : this(column)
        {
            FormFieldName = formFieldName;
            FormFieldValue = formFieldValue;
        }

        public DataTableColumnDefinition Column { get; }

        public string? FormFieldName { get; set; }
        public object? FormFieldValue { get; set; }

        public sealed class TextFilter : DataTableColumnFilterDefinition
        {
            public TextFilter(DataTableColumnDefinition column, string formFieldName, object? formFieldValue) : base(column, formFieldName, formFieldValue) { }

            public LocalizedHtmlString? PlaceholderText { get; set; }
        }
    }
}
