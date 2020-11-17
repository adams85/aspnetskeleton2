using WebApp.UI.Models.DataTables;

namespace WebApp.UI.Helpers.Views
{
    public interface IDataTableHelpers
    {
        void Table<TItem>(DataTableDefinition<TItem> tableDefinition);

        void TableHeaderRow<TItem>(DataTableDefinition<TItem> tableDefinition);
        void PageSizeSelector<TItem>(DataTableDefinition<TItem> tableDefinition);

        void ColumnHeaderRow<TItem>(DataTableDefinition<TItem> tableDefinition);
        void ColumnHeader<TItem>(DataTableColumnDefinition<TItem> columnDefinition);

        void ColumnFilterRow<TItem>(DataTableDefinition<TItem> tableDefinition);
        void ColumnFilter<TItem>(DataTableColumnDefinition<TItem> columnDefinition);
        void TextColumnFilter<TItem>(DataTableColumnFilter.Text columnFilter, DataTableColumnDefinition<TItem> columnDefinition);

        void DataRow<TItem>(TItem item, DataTableDefinition<TItem> tableDefinition);
        void DataCell<TItem>(TItem item, DataTableColumnDefinition<TItem> columnDefinition);
        void NoDataRow<TItem>(DataTableDefinition<TItem> tableDefinition);

        void TableFooterRow<TItem>(DataTableDefinition<TItem> tableDefinition);
    }
}
