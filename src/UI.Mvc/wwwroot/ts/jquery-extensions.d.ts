interface DataTableRefreshParams {
    debounce?: boolean;
}

type DataTableRefreshEventHandler = (this: HTMLElement, t: JQuery.TriggeredEvent<HTMLElement, undefined>, params?: DataTableRefreshParams) => any;

type DataTableEditActionEventHandler = JQuery.EventHandler<HTMLElement>;

interface DataTableOptions {
    initializeTable($table: JQuery, url: string, initial: boolean): void;
    getRefreshButton($table: JQuery): JQuery;
    handleRefreshError($table: JQuery, jqXhr: JQueryXHR, error: JQuery.Ajax.ErrorTextStatus): void;
    subscribeToPageSizeChange($table: JQuery, handler: DataTableRefreshEventHandler): void;
    subscribeToPageIndexChange($table: JQuery, handler: DataTableRefreshEventHandler): void;
    subscribeToColumnSort($table: JQuery, handler: DataTableRefreshEventHandler): void;
    subscribeToColumnFilter($table: JQuery, handler: DataTableRefreshEventHandler): void;
    subscribeToEditAction($table: JQuery, handler: DataTableRefreshEventHandler): void;
    handleEditActionError($table: JQuery, jqXhr: JQueryXHR, error: JQuery.Ajax.ErrorTextStatus): void;
    initializeEditModal($modal: JQuery, url: string, initial: boolean): void;
    subscribeToEditModalSubmit($modal: JQuery, handler: DataTableEditActionEventHandler): void;
    subscribeToEditModalCancel($modal: JQuery, handler: DataTableEditActionEventHandler): void;
    enableEditModalButtons($modal: JQuery, value: boolean): void;
    handleEditModalSubmitError($modal: JQuery, jqXhr: JQueryXHR, error: JQuery.Ajax.ErrorTextStatus): void;
}

interface DataTable {
    (this: JQuery, actionOrOptions?: string | Partial<DataTableOptions>): void;
    defaults: DataTableOptions;
}

interface JQuery {
    dataTable: DataTable;
}
