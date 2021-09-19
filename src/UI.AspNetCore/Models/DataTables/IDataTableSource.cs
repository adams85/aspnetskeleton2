using System;
using WebApp.Service;

namespace WebApp.UI.Models.DataTables
{
    public interface IDataTableSource
    {
        IListQuery Query { get; }
        IListResult Result { get; }
        Type ItemType { get; }
    }
}
