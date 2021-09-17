﻿function getSidebarState($sidebar) {
    var isVisible = !!$sidebar.hasClass("c-sidebar-lg-show");
    var isMinimized = !!$sidebar.hasClass("c-sidebar-minimized");
    return (+isMinimized << 1) | +isVisible;
}

function updateSidebarCookie($sidebar, pageContext) {
    document.cookie = pageContext.SidebarStateCookieName + "=" + getSidebarState($sidebar) + ";Max-Age=2147483647;SameSite=lax;Path=" + pageContext.RootPath;
}

export function initialize(pageContext) {
    $(document).on("classtoggle", "#sidebar", function (e) {
        updateSidebarCookie($(e.target), pageContext);
    });
}
