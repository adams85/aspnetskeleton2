(function (context) {
    function getSidebarState($sidebar) {
        var isVisible = !!$sidebar.hasClass("c-sidebar-lg-show");
        var isMinimized = !!$sidebar.hasClass("c-sidebar-minimized");
        return (+isMinimized << 1) | +isVisible;
    }

    function updateSidebarCookie($sidebar) {
        document.cookie = context.SidebarStateCookieName + "=" + getSidebarState($sidebar) + ";Max-Age=2147483647;SameSite=lax;Path=" + context.RootPath;
    }

    $(document).on("classtoggle", "#sidebar", function (e) {
        updateSidebarCookie($(e.target));
    });
})(DashboardPageContext);
