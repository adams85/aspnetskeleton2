interface DashboardPageContext {
    rootPath: string;
    sidebarStateCookieName: string;
    dataTableOptions?: Partial<DataTableOptions>;
}

interface Window {
    DashboardPageContext: DashboardPageContext;
}
