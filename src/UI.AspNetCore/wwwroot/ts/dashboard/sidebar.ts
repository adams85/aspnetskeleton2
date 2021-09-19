function getSidebarState($sidebar: JQuery) {
    const isVisible = !!$sidebar.hasClass("c-sidebar-lg-show");
    const isMinimized = !!$sidebar.hasClass("c-sidebar-minimized");
    return (+isMinimized << 1) | +isVisible;
}

function updateSidebarCookie($sidebar: JQuery, pageContext: DashboardPageContext) {
    document.cookie = pageContext.sidebarStateCookieName + "=" + getSidebarState($sidebar) + ";Max-Age=2147483647;SameSite=lax;Path=" + pageContext.rootPath;
}

export function initialize(pageContext: DashboardPageContext) {
    $(document).on("classtoggle", "#sidebar", function (e) {
        updateSidebarCookie($(e.target), pageContext);
    });
}
