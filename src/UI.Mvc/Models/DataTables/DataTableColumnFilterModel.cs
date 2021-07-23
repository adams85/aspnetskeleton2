using System.Collections.Generic;
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

        public DataTableColumnModel Column { get; }

        public abstract IEnumerable<string> FormFieldNames { get; }

        protected internal abstract void RenderDefault(IDataTableHelpers helpers);

        public sealed class TextFilter : DataTableColumnFilterModel
        {
            public TextFilter(DataTableColumnModel column, string formFieldName, object? formFieldValue) : base(column) =>
                (FormFieldName, FormFieldValue) = (formFieldName, formFieldValue);

            public override IEnumerable<string> FormFieldNames => new[] { FormFieldName };

            public string FormFieldName { get; set; }
            public object? FormFieldValue { get; set; }

            public LocalizedHtmlString? PlaceholderText { get; set; }

            protected internal override void RenderDefault(IDataTableHelpers helpers) => helpers.TextColumnFilter(this);
        }
    }
}
