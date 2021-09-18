import * as sidebar from "./sidebar.js";
import * as dataTable from "../data-table.js";

const pageContext = window.DashboardPageContext;

sidebar.initialize(pageContext);
dataTable.initialize(pageContext.dataTableOptions);
