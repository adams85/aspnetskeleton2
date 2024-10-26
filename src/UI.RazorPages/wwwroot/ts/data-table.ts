import { debounce, getAssociatedForm } from "./utils.js";

const NAME = "dataTable";
const DATA_KEY = "app." + NAME;
const EVENT_KEY = "." + DATA_KEY;
const SELECTOR = ".data-table-wrapper > .data-table";

// Plugin implementation.
class Plugin {
    constructor($table: JQuery, options: DataTableOptions, initial: boolean) {
        const $refreshButton = options.getRefreshButton($table);
        if ($refreshButton.length !== 1) {
            console.error("Data table has no single associated refresh button.", $table[0]);
            return;
        }

        const $filterForm = getAssociatedForm($refreshButton);
        if (!$filterForm.length) {
            console.error("Data table has no associated filter form.", $table[0]);
            return;
        }

        class AjaxCoordinator {
            constructor() {
                let pendingRefreshXhr: JQueryXHR | undefined,
                    pendingEditSubmitXhr: JQueryXHR | undefined;

                function tableAjaxRequest(method: string, url: string, data: any, success: JQuery.Ajax.SuccessCallback<any>, error: JQuery.Ajax.ErrorCallback<any>) {
                    pendingRefreshXhr = $.ajax({
                        type: method,
                        url: url,
                        data: data,
                        beforeSend: () => {
                            $table.addClass("loading");
                        },
                        success: success,
                        error: error,
                        complete: () => {
                            pendingRefreshXhr = void 0;
                            $table.removeClass("loading");
                        }
                    });
                }

                const refreshCore = function (url: string, data: any) {
                    tableAjaxRequest("GET", url, data,
                        data => {
                            const $newTable = $(data).children(SELECTOR);
                            $newTable.addClass("loading");
                            $table.parent().replaceWith($newTable.parent());
                            ensurePlugin($newTable, options, false);
                            // HACK: https://stackoverflow.com/questions/14654803/css-transition-not-working-after-element-is-appended
                            $newTable.offset();
                            $newTable.removeClass("loading");
                        },
                        (jqXhr, error) => options.handleRefreshError($table, jqXhr, error));
                };

                const debouncedRefreshCore = debounce(refreshCore, 500);

                this.refresh = function (url, $form, debounce) {
                    if (pendingRefreshXhr)
                        pendingRefreshXhr.abort();

                    (debounce ? debouncedRefreshCore : refreshCore)(url, $form ? $form.serialize() : void 0);
                }

                function initializeEditModal($modal: JQuery, url: string, initial: boolean) {
                    $modal.data(DATA_KEY, $table[0]);

                    options.initializeEditModal($modal, url, initial);

                    options.subscribeToEditModalSubmit($modal, function (e) {
                        e.preventDefault();

                        if (pendingRefreshXhr || pendingEditSubmitXhr)
                            return;

                        const $button = $(this), $form = getAssociatedForm($button);

                        const formMethod = $button.attr("formmethod") || $form.attr("method"),
                            formAction = $button.attr("formaction") || $form.attr("action"),
                            formEncType = $button.attr("formenctype") || $form.attr("enctype");

                        let originalButtonHtml: string | undefined, hideOnComplete = false;

                        pendingEditSubmitXhr = $.ajax({
                            type: formMethod || "POST",
                            url: formAction || url,
                            contentType: formEncType,
                            data: $form.serialize(),
                            beforeSend: () => {
                                options.enableEditModalButtons($modal, false);
                                originalButtonHtml = $button.html();
                                const loadingText = $button.attr("data-loading-text");
                                if (loadingText)
                                    $button.html('<i class="fa fa-circle-o-notch fa-spin"></i> ' + loadingText);
                            },
                            success: (data, status, jqXhr) => {
                                if (jqXhr.status === 200) {
                                    $modal.html($(data).html());
                                    initializeEditModal($modal, url, false);
                                    $modal.modal('handleUpdate');
                                }
                                else if (jqXhr.status === 204) {
                                    hideOnComplete = true;
                                    $modal.one("hidden.bs.modal", function () { $table.dataTable("refresh"); });
                                }
                            },
                            error: function (jqXhr, error) {
                                options.handleEditModalSubmitError($modal, jqXhr, error);
                            },
                            complete: function () {
                                pendingEditSubmitXhr = void 0;
                                if (hideOnComplete) {
                                    $modal.modal("hide");
                                }
                                else {
                                    $button.html(originalButtonHtml!);
                                    options.enableEditModalButtons($modal, true);
                                }
                            }
                        });
                    });

                    options.subscribeToEditModalCancel($modal, function (e) {
                        e.preventDefault();
                        $modal.modal("hide");
                    });
                }

                this.editItem = function (url, data) {
                    if (pendingRefreshXhr)
                        pendingRefreshXhr.abort();

                    tableAjaxRequest("GET", url, data,
                        function (data) {
                            const $modal = $(data);
                            $("body").append($modal);
                            initializeEditModal($modal, url, true);
                            $modal.one("shown.bs.modal", function () { $("input[autofocus]", $modal).trigger("focus"); });
                            $modal.on("hide.bs.modal", function (e) {
                                if (pendingEditSubmitXhr) {
                                    e.preventDefault();
                                    e.stopImmediatePropagation();
                                    return false;
                                }
                            })
                            $modal.one("hidden.bs.modal", function () { $modal.remove(); });
                            $modal.modal({ backdrop: "static" });
                        },
                        function (jqXhr, error) {
                            options.handleEditActionError($table, jqXhr, error);
                        });
                }
            }

            public refresh: (url: string, $form: JQuery | undefined, debounce: boolean) => void;
            public editItem: (url: string, data?: any) => void;
        }

        const ajaxCoordinator = new AjaxCoordinator();

        const refreshUrl = $refreshButton.attr("formaction") || $filterForm.attr("action") || $(location).attr("pathname")!;

        $filterForm.on("submit", function (e, params: DataTableRefreshParams) {
            e.preventDefault();
            ajaxCoordinator.refresh(refreshUrl, $filterForm, !!(params && params.debounce));
        });

        const filterInputChangeHandler: DataTableRefreshEventHandler = function (e, params) {
            $filterForm.trigger("submit", [params]);
        }

        const pageAndSortHandler: DataTableRefreshEventHandler = function (e, params) {
            e.preventDefault();
            ajaxCoordinator.refresh($(this).attr("href")!, void 0, !!(params && params.debounce));
        }

        const editActionHandler: DataTableRefreshEventHandler = function (e, params) {
            e.preventDefault();
            ajaxCoordinator.editItem($(this).attr("href")!);
        }

        options.subscribeToPageSizeChange($table, filterInputChangeHandler);
        options.subscribeToPageIndexChange($table, pageAndSortHandler);
        options.subscribeToColumnSort($table, pageAndSortHandler);
        options.subscribeToColumnFilter($table, filterInputChangeHandler);
        options.subscribeToEditAction($table, editActionHandler);

        this.refresh = function () {
            $filterForm.trigger("submit");
        }

        options.initializeTable($table, refreshUrl, initial);
    }

    public refresh = () => { };
}

function ensurePlugin($table: JQuery, options: string | Partial<DataTableOptions> | undefined, initial: boolean) {
    let data = $table.data(DATA_KEY);
    if (!data) {
        options = typeof options === 'object' ? options : void 0 as unknown as Partial<DataTableOptions>;
        $table.data(DATA_KEY, data = new Plugin($table, $.extend({}, $.fn[NAME].defaults, options), initial));
    }
    return data;
}

// Plugin definition.
$.fn[NAME] = function (this: JQuery, options?: string | Partial<DataTableOptions>) {
    const args = Array.prototype.slice.call(arguments, 1);

    return this.each(function () {
        const data = ensurePlugin($(this), options, true);

        if (typeof options === 'string')
            data[options].apply(data, args);
    });
} as unknown as DataTable;

// Plugin defaults.
$.fn[NAME].defaults = {
    initializeTable: function ($table, url, initial) { },
    getRefreshButton: function ($table) { return $(".refresh-table-btn", $table); },
    handleRefreshError: function ($table, jqXhr, error) {
        // https://stackoverflow.com/questions/26273585/how-to-catch-ajax-abort-in-jquery
        if (error !== 'abort')
            $table.addClass("server-error");
    },
    subscribeToPageSizeChange: function ($table, handler) {
        const $elements = $("select.page-size-selector", $table);
        $elements.on("change" + EVENT_KEY, handler);
    },
    subscribeToPageIndexChange: function ($table, handler) {
        const $elements = $("a.page-link[href]", $table);
        $elements.on("click" + EVENT_KEY, handler);
    },
    subscribeToColumnSort: function ($table, handler) {
        const $elements = $("a.title[href]", $(".column-header-row", $table));
        $elements.on("click" + EVENT_KEY, handler);
    },
    subscribeToColumnFilter: function ($table, handler) {
        const $elements = $("input, select", $(".column-filter-row", $table));
        $elements.each(function () {
            const $input = $(this);
            let inputType: string | undefined;
            if ($input.is("select") ||
                (inputType = $input.attr("type")) && ((inputType = inputType.toLowerCase()) == "checkbox" || inputType == "radio")) {
                $input.on("change" + EVENT_KEY, handler);
            }
            else
                $input.on("keyup" + EVENT_KEY, function (e) { handler.call(this, e, { debounce: true }); });
        });
    },
    subscribeToEditAction: function ($table, handler) {
        const $elements = $("a.create-item-btn", $(".table-header-row", $table)).add($("a.edit-item-btn, a.delete-item-btn", $(".content-row .control-column", $table)));
        $elements.on("click" + EVENT_KEY, handler);
    },
    handleEditActionError: function ($table, jqXhr, error) {
        // https://stackoverflow.com/questions/26273585/how-to-catch-ajax-abort-in-jquery
        if (error !== 'abort')
            $table.addClass("server-error");
    },
    initializeEditModal: function ($modal, url, initial) { },
    subscribeToEditModalSubmit: function ($modal, handler) {
        const $elements = $(".submit-btn", $modal);
        $elements.on("click" + EVENT_KEY, handler);
    },
    subscribeToEditModalCancel: function ($modal, handler) {
        const $elements = $(".cancel-btn", $modal);
        $elements.on("click" + EVENT_KEY, handler);
    },
    enableEditModalButtons: function ($modal, value) {
        const $elements = $(".submit-btn, .cancel-btn", $modal);
        if (value)
            $elements.removeClass("disabled");
        else
            $elements.addClass("disabled");
        $elements.attr({ "disabled": !value, "aria-disabled": "" + !value});
    },
    handleEditModalSubmitError: function ($modal, jqXhr, error) {
        const $modalBody = $(".modal-body", $modal);
        if (!$modalBody.has(".server-error-alert").length) {
            const $table = $($modal.data(DATA_KEY)),
                $alert = $(".server-error-alert", $table);
            $modalBody.prepend($alert.clone());
        }
    }
};

export function initialize(options?: Partial<DataTableOptions> | { selector: string; options?: Partial<DataTableOptions> }[]) {
    $(document).ready(function () {
        if ($.isArray(options)) {
            for (let i = 0; i < options.length; i++) {
                const item = options[i];
                $(item.selector).dataTable(item.options);
            }
        }
        else
            $(SELECTOR).dataTable(options);
    });
}
