// based on: https://davidwalsh.name/javascript-debounce-function
// Returns a function, that, as long as it continues to be invoked, will not
// be triggered. The function will be called after it stops being called for
// N milliseconds. If `immediate` is passed, trigger the function on the
// leading edge, instead of the trailing.
export function debounce(func, wait, immediate) {
    var timeout;

    return function () {
        var context = this, args = arguments, callNow = immediate && !timeout;

        clearTimeout(timeout);

        timeout = setTimeout(function () {
            timeout = void 0;
            if (!immediate)
                func.apply(context, args);
        }, wait);

        if (callNow)
            func.apply(context, args);
    };
}

export function getAssociatedForm($element) {
    var formName = $element.attr("form");
    return formName ? $("#" + formName) : $element.closest("form");
}
