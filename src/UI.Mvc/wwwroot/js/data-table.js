import { debounce, getAssociatedForm } from "./utils.js";

var NAME = "dataTable";
var DATA_KEY = "app." + NAME;
var EVENT_KEY = "." + DATA_KEY;
var SELECTOR = ".data-table-wrapper > .data-table";

// Plugin implementation.
function Plugin($table, options, initial) {
    options = $.extend({}, $.fn[NAME].defaults, options);

    var $refreshButton = options.getRefreshButton($table);
    if ($refreshButton.length !== 1) {
        console.error("Data table has no single associated refresh button.", $table[0]);
        return;
    }

    var $filterForm = getAssociatedForm($refreshButton);
    if (!$filterForm.length) {
        console.error("Data table has no associated filter form.", $table[0]);
        return;
    }

    function AjaxCoordinator() {
        var pendingRefreshXhr, pendingEditSubmitXhr;

        function tableAjaxRequest(method, url, data, success, error) {
            pendingRefreshXhr = $.ajax({
                type: method,
                url: url,
                data: data,
                beforeSend: function () {
                    $table.addClass("loading");
                },
                success: success,
                error: error,
                complete: function () {
                    pendingRefreshXhr = void 0;
                    $table.removeClass("loading");
                }
            });
        }

        var refreshCore = function (url, data) {
            tableAjaxRequest("GET", url, data,
                function (data) {
                    var $newTable = $(data).children(SELECTOR);
                    $newTable.addClass("loading");
                    $table.parent().replaceWith($newTable.parent());
                    ensurePlugin($newTable, options, false);
                    // HACK: https://stackoverflow.com/questions/14654803/css-transition-not-working-after-element-is-appended
                    $newTable.offset();
                    $newTable.removeClass("loading");
                },
                function (jqXhr, error) {
                    options.handleRefreshError($table, jqXhr, error);
                });
        };

        var debouncedRefreshCore = debounce(refreshCore, 500);

        this.refresh = function (url, $form, debounce) {
            if (pendingRefreshXhr)
                pendingRefreshXhr.abort();

            (debounce ? debouncedRefreshCore : refreshCore)(url, $form ? $form.serialize() : void 0);
        }

        function initializeEditModal($modal, url, initial) {
            $modal.data(DATA_KEY, $table[0]);

            options.initializeEditModal($modal, url, initial);

            options.subscribeToEditModalSubmit($modal, function (e, params) {
                e.preventDefault();

                if (pendingRefreshXhr || pendingEditSubmitXhr)
                    return;

                var $button = $(this), $form = getAssociatedForm($button);

                var formMethod = $button.attr("formmethod") || $form.attr("method"),
                    formAction = $button.attr("formaction") || $form.attr("action"),
                    formEncType = $button.attr("formenctype") || $form.attr("enctype");

                var originalButtonHtml, hideOnComplete = false;

                pendingEditSubmitXhr = $.ajax({
                    type: formMethod || "POST",
                    url: formAction || url,
                    contentType: formEncType,
                    data: $form.serialize(),
                    beforeSend: function () {
                        options.enableEditModalButtons($modal, false);
                        originalButtonHtml = $button.html();
                        var loadingText = $button.attr("data-loading-text");
                        if (loadingText)
                            $button.html('<i class="fa fa-circle-o-notch fa-spin"></i> ' + loadingText);
                    },
                    success: function (data, status, jqXhr) {
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
                            $button.html(originalButtonHtml);
                            options.enableEditModalButtons($modal, true);
                        }
                    }
                });
            });

            options.subscribeToEditModalCancel($modal, function (e, params) {
                e.preventDefault();
                $modal.modal("hide");
            });
        }

        this.editItem = function (url, data) {
            if (pendingRefreshXhr)
                pendingRefreshXhr.abort();

            tableAjaxRequest("GET", url, data,
                function (data) {
                    var $modal = $(data);
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

    var ajaxCoordinator = new AjaxCoordinator();

    var refreshUrl = $refreshButton.attr("formaction") || $filterForm.attr("action") || $(location).attr("pathname");

    $filterForm.on("submit", function (e, params) {
        e.preventDefault();
        ajaxCoordinator.refresh(refreshUrl, $filterForm, params && params.debounce);
    });

    function filterInputChangeHandler(e, params) {
        $filterForm.trigger("submit", [params]);
    }

    function pageAndSortHandler(e, params) {
        e.preventDefault();
        ajaxCoordinator.refresh($(this).attr("href"), void 0, params && params.debounce);
    }

    function editActionHandler(e, params) {
        e.preventDefault();
        ajaxCoordinator.editItem($(this).attr("href"));
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

function ensurePlugin($table, options, initial) {
    var data = $table.data(DATA_KEY);
    if (!data)
        $table.data(DATA_KEY, data = new Plugin($table, typeof options === 'object' && options, initial));
    return data;
}

// Plugin definition.
$.fn[NAME] = function (options) {
    var args = Array.prototype.slice.call(arguments, 1);

    return this.each(function () {
        var data = ensurePlugin($(this), options, true);

        if (typeof options === 'string')
            data[options].apply(data, args);
    });
}

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
        var $elements = $("select.page-size-selector", $table);
        $elements.on("change" + EVENT_KEY, handler);
    },
    subscribeToPageIndexChange: function ($table, handler) {
        var $elements = $("a.page-link[href]", $table);
        $elements.on("click" + EVENT_KEY, handler);
    },
    subscribeToColumnSort: function ($table, handler) {
        var $elements = $("a.title[href]", $(".column-header-row", $table));
        $elements.on("click" + EVENT_KEY, handler);
    },
    subscribeToColumnFilter: function ($table, handler) {
        var $elements = $("input, select", $(".column-filter-row", $table));
        $elements.each(function () {
            var $input = $(this), inputType;
            if ($input.is("select") || (inputType = $input.attr("type").toLowerCase()) == "checkbox" || inputType == "radio")
                $input.on("change" + EVENT_KEY, handler);
            else
                $input.on("keyup" + EVENT_KEY, function (e) { handler.call(this, e, { debounce: true }); });
        });
    },
    subscribeToEditAction: function ($table, handler) {
        var $elements = $("a.create-item-btn", $(".table-header-row", $table)).add($("a.edit-item-btn, a.delete-item-btn", $(".content-row .control-column", $table)));
        $elements.on("click" + EVENT_KEY, handler);
    },
    handleEditActionError: function ($table, jqXhr, error) {
        // https://stackoverflow.com/questions/26273585/how-to-catch-ajax-abort-in-jquery
        if (error !== 'abort')
            $table.addClass("server-error");
    },
    initializeEditModal: function ($modal, url, initial) { },
    subscribeToEditModalSubmit: function ($modal, handler) {
        var $elements = $(".submit-btn", $modal);
        $elements.on("click" + EVENT_KEY, handler);
    },
    subscribeToEditModalCancel: function ($modal, handler) {
        var $elements = $(".cancel-btn", $modal);
        $elements.on("click" + EVENT_KEY, handler);
    },
    enableEditModalButtons: function ($modal, value) {
        var $elements = $(".submit-btn, .cancel-btn", $modal);
        if (value)
            $elements.removeClass("disabled");
        else
            $elements.addClass("disabled");
        $elements.prop("disabled", !value);
    },
    handleEditModalSubmitError: function ($modal, jqXhr, error) {
        var $modalBody = $(".modal-body", $modal);
        if (!$modalBody.has(".server-error-alert").length) {
            var $table = $($modal.data(DATA_KEY)),
                $alert = $(".server-error-alert", $table);
            $modalBody.prepend($alert.clone());
        }
    }
};

export function initialize(options) {
    $(document).ready(function () {
        if ($.isArray(options)) {
            for (var i = 0; i < options.length; i++) {
                var item = options[i];
                $(item.selector).dataTable(item.options);
            }
        }
        else
            $(SELECTOR).dataTable(options);
    });
}
