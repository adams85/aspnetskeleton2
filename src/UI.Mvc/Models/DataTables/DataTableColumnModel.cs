using System;
using Karambolo.Common;
using Microsoft.AspNetCore.Html;
using WebApp.UI.Helpers.Views;

namespace WebApp.UI.Models.DataTables
{
    public abstract class DataTableColumnModel
    {
        protected DataTableColumnModel(DataTableModel table)
        {
            Table = table;
        }

        public DataTableModel Table { get; }

        public string? Title { get; set; }

        public bool CanSort { get; set; }
        public string? OrderKeyPropertyPath { get; set; }
        public string? AscendingOrderIconCssClass { get; set; }
        public string? DescendingOrderIconCssClass { get; set; }

        public Func<DataTableColumnModel, IHtmlContent>? HeaderCellTemplate { get; set; }

        public bool CanFilter { get; set; }

        public DataTableColumnFilterModel? Filter { get; set; }

        public Func<DataTableColumnModel, DataTableColumnFilterModel> FilterFactory
        {
            set => Filter = value(this);
        }

        private Func<DataTableColumnModel, IHtmlContent>? _filterCellTemplate;
        public Func<DataTableColumnModel, IHtmlContent>? FilterCellTemplate
        {
            get => _filterCellTemplate;
            set => (_filterCellTemplate, _renderFilterCell) = (value, null);
        }

        private Action<IDataTableHelpers>? _renderFilterCell;
        public Action<IDataTableHelpers> RenderFilterCell => _renderFilterCell ??=
            FilterCellTemplate != null ?
            helpers => helpers.Write(FilterCellTemplate(this)) :
            new Action<IDataTableHelpers>(RenderFilterCellDefault);

        protected virtual void RenderFilterCellDefault(IDataTableHelpers helpers) =>
            helpers.ColumnFilterCell(this, Filter != null ? Filter.RenderDefault : CachedDelegates.Noop<IDataTableHelpers>.Action);

        private Func<(object Item, DataTableColumnModel ColumnModel), IHtmlContent>? _contentCellTemplate;
        public Func<(object Item, DataTableColumnModel ColumnModel), IHtmlContent>? ContentCellTemplate
        {
            get => _contentCellTemplate;
            set => (_contentCellTemplate, _renderContentCell) = (value, null);
        }

        private Action<IDataTableHelpers, object>? _renderContentCell;
        public Action<IDataTableHelpers, object> RenderContentCell => _renderContentCell ??=
            ContentCellTemplate != null ?
            (helpers, item) => helpers.Write(ContentCellTemplate((item, this))) :
            new Action<IDataTableHelpers, object>(RenderContentCellDefault);

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
            public ControlColumn(DataTableModel table) : base(table) { }

            public Func<object, bool> CanEditRow { get; set; } = CachedDelegates.True<object>.Func;
            public Func<object, bool> CanDeleteRow { get; set; } = CachedDelegates.True<object>.Func;

            protected override void RenderContentCellDefault(IDataTableHelpers helpers, object item) => helpers.ControlContentCell(item, this);
        }
    }
}
