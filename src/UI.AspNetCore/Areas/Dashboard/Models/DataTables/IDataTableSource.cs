using System;
using WebApp.Service;

namespace WebApp.UI.Areas.Dashboard.Models.DataTables
{
    public interface IDataTableSource
    {
        IListQuery Query { get; }
        IListResult Result { get; }
        Type ItemType { get; }
    }
}
