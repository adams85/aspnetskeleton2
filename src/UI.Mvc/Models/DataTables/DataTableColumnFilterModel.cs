using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Helpers.Views;

namespace WebApp.UI.Models.DataTables
{
    public abstract class DataTableColumnFilterModel
    {
        protected DataTableColumnFilterModel(DataTableColumnModel column)
        {
            Column = column;
        }

        protected DataTableColumnFilterModel(DataTableColumnModel column, string formFieldName, object? formFieldValue) : this(column)
        {
            FormFieldName = formFieldName;
            FormFieldValue = formFieldValue;
        }

        public DataTableColumnModel Column { get; }

        public string? FormFieldName { get; set; }
        public object? FormFieldValue { get; set; }

        protected internal abstract void RenderDefault(IDataTableHelpers helpers);

        public sealed class TextFilter : DataTableColumnFilterModel
        {
            public TextFilter(DataTableColumnModel column, string formFieldName, object? formFieldValue) : base(column, formFieldName, formFieldValue) { }

            public LocalizedHtmlString? PlaceholderText { get; set; }

            protected internal override void RenderDefault(IDataTableHelpers helpers) => helpers.TextColumnFilter(this);
        }
    }
}
