import * as sidebar from "./sidebar.js";
import * as dataTable from "../data-table.js";

var pageContext = window.DashboardPageContext;

sidebar.initialize(pageContext);
dataTable.initialize(pageContext.dataTableOptions);
