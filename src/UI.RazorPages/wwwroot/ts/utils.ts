// based on: https://davidwalsh.name/javascript-debounce-function
// Returns a function, that, as long as it continues to be invoked, will not
// be triggered. The function will be called after it stops being called for
// N milliseconds. If `immediate` is passed, trigger the function on the
// leading edge, instead of the trailing.
export function debounce<TFunc extends Function>(func: TFunc, wait?: number, immediate?: boolean) {
    let timeout: number | undefined;

    return function (this: any) {
        const context = this, args = arguments, callNow = immediate && !timeout;

        clearTimeout(timeout);

        timeout = setTimeout(function () {
            timeout = void 0;
            if (!immediate)
                func.apply(context, args as unknown as any[]);
        }, wait);

        if (callNow)
            func.apply(context, args as unknown as any[]);
    } as unknown as TFunc;
}

export function getAssociatedForm($element: JQuery) {
    const formName = $element.attr("form");
    return formName ? $("#" + formName) : $element.closest("form");
}
