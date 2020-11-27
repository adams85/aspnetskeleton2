using WebApp.UI.Models.DataTables;

namespace WebApp.UI.Helpers.Views
{
    public interface IDataTableHelpers
    {
        void Table(DataTableDefinition tableDefinition);

        void TableHeaderRow(DataTableDefinition tableDefinition);
        void PageSizeSelector(DataTableDefinition tableDefinition);

        void ColumnHeaderRow(DataTableDefinition tableDefinition);
        void ColumnHeader(DataTableColumnDefinition columnDefinition);

        void ColumnFilterRow(DataTableDefinition tableDefinition);
        void ColumnFilter(DataTableColumnDefinition columnDefinition);
        void TextColumnFilter(DataTableColumnFilterDefinition.TextFilter filterDefinition);

        void DataRow(object item, DataTableDefinition tableDefinition);
        void DataCell(object item, DataTableColumnDefinition columnDefinition);
        void BoundDataCell(object item, DataTableColumnDefinition.DataColumn columnDefinition);
        void NoDataRow(DataTableDefinition tableDefinition);

        void TableFooterRow(DataTableDefinition tableDefinition);
    }
}
