using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Areas.Dashboard.Pages.Shared.Helpers;

namespace WebApp.UI.Areas.Dashboard.Models.DataTables;

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

        public string FormFieldName { get; init; }
        public object? FormFieldValue { get; init; }

        public LocalizedHtmlString? PlaceholderText { get; init; }

        protected internal override void RenderDefault(IDataTableHelpers helpers) => helpers.TextColumnFilter(this);
    }
}
