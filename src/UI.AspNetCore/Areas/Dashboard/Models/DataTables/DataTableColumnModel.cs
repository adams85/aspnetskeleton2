using System;
using Karambolo.Common;
using Microsoft.AspNetCore.Html;
using WebApp.UI.Areas.Dashboard.Pages.Shared.Helpers;

namespace WebApp.UI.Areas.Dashboard.Models.DataTables
{
    public abstract class DataTableColumnModel
    {
        protected DataTableColumnModel(DataTableModel table)
        {
            Table = table;
        }

        public DataTableModel Table { get; }

        public string? Title { get; init; }

        public bool CanSort { get; init; }
        public string? OrderKeyPropertyPath { get; init; }
        public string? AscendingOrderIconCssClass { get; init; }
        public string? DescendingOrderIconCssClass { get; init; }

        public string? HeaderCellCssClasses { get; init; }

        public Func<DataTableColumnModel, IHtmlContent>? HeaderCellTemplate { get; init; }

        public bool CanFilter { get; init; }

        private DataTableColumnFilterModel? _filter;
        public DataTableColumnFilterModel? Filter
        {
            get => _filter;
            init => _filter = value;
        }

        public Func<DataTableColumnModel, DataTableColumnFilterModel> FilterFactory
        {
            init => _filter = value(this);
        }

        public string? FilterCellCssClasses { get; init; }

        private Func<DataTableColumnModel, IHtmlContent>? _filterCellTemplate;
        public Func<DataTableColumnModel, IHtmlContent>? FilterCellTemplate
        {
            get => _filterCellTemplate;
            init => (_filterCellTemplate, _renderFilterCell) = (value, null);
        }

        private Action<IDataTableHelpers>? _renderFilterCell;
        public Action<IDataTableHelpers> RenderFilterCell => _renderFilterCell ??=
            FilterCellTemplate != null ?
            helpers => helpers.Write(FilterCellTemplate!(this)) :
            RenderFilterCellDefault;

        protected virtual void RenderFilterCellDefault(IDataTableHelpers helpers) =>
            helpers.ColumnFilterCell(this, Filter != null ? Filter.RenderDefault : CachedDelegates.Noop<IDataTableHelpers>.Action);

        public string? ContentCellCssClasses { get; init; }

        private Func<(object Item, DataTableColumnModel ColumnModel), IHtmlContent>? _contentCellTemplate;
        public Func<(object Item, DataTableColumnModel ColumnModel), IHtmlContent>? ContentCellTemplate
        {
            get => _contentCellTemplate;
            init => (_contentCellTemplate, _renderContentCell) = (value, null);
        }

        private Action<IDataTableHelpers, object>? _renderContentCell;
        public Action<IDataTableHelpers, object> RenderContentCell => _renderContentCell ??=
            ContentCellTemplate != null ?
            (helpers, item) => helpers.Write(ContentCellTemplate!((item, this))) :
            RenderContentCellDefault;

        protected abstract void RenderContentCellDefault(IDataTableHelpers helpers, object item);

        public sealed class DataColumn : DataTableColumnModel
        {
            public DataColumn(DataTableModel table, DataTableColumnBinding binding) : base(table)
            {
                if (table.ItemType != binding.ItemType)
                    throw new ArgumentException("Item type of table model and column binding mismatch.", nameof(table));

                Binding = binding;

                CanSort = true;
                CanFilter = true;
            }

            public DataTableColumnBinding Binding { get; }

            protected override void RenderContentCellDefault(IDataTableHelpers helpers, object item) => helpers.DataContentCell(item, this);
        }

        public sealed class ControlColumn : DataTableColumnModel
        {
            public ControlColumn(DataTableModel table) : base(table)
            {
                ContentCellCssClasses = "control-column";
            }

            public Func<object, bool> CanEditRow { get; init; } = CachedDelegates.True<object>.Func;
            public Func<object, bool> CanDeleteRow { get; init; } = CachedDelegates.True<object>.Func;

            protected override void RenderContentCellDefault(IDataTableHelpers helpers, object item) => helpers.ControlContentCell(item, this);
        }
    }
}
