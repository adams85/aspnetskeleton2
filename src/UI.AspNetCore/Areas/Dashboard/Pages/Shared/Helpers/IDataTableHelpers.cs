using System;
using WebApp.UI.Areas.Dashboard.Models.DataTables;

namespace WebApp.UI.Areas.Dashboard.Pages.Shared.Helpers;

public interface IDataTableHelpers
{
    void Table(DataTableModel tableModel);
    void TableFilterForm(DataTableModel tableModel);

    void TableHeader(DataTableModel tableModel);
    void TableHeaderRow(DataTableModel tableModel);
    void PageSizeSelector(DataTableModel tableModel);

    void ColumnHeaderRow(DataTableModel tableModel);
    void ColumnHeaderCell(DataTableColumnModel columnModel);

    void ColumnFilterRow(DataTableModel tableModel);
    void ColumnFilterCell(DataTableColumnModel columnModel);
    void ColumnFilterCell(DataTableColumnModel columnModel, Action<IDataTableHelpers> renderFilter);
    void TextColumnFilter(DataTableColumnFilterModel.TextFilter filterModel);

    void TableBody(DataTableModel tableModel);
    void ContentRow(object item, DataTableModel tableModel);
    void ContentCell(object item, DataTableColumnModel columnModel);
    void DataContentCell(object item, DataTableColumnModel.DataColumn columnModel);
    void ControlContentCell(object item, DataTableColumnModel.ControlColumn columnModel);
    void NoContentRow(DataTableModel tableModel);

    void TableFooter(DataTableModel tableModel);
    void TableFooterRow(DataTableModel tableModel);

    void Write(object value);
}
