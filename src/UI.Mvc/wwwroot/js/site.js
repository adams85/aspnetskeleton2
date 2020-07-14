var page = {
    init: function () {
        $(document).on("click", "button[x-confirm-delete]", function (e) {
            if (!confirm(page.confirmDeleteMessage))
                e.preventDefault();
        });

        $(document).ready(function() {
            $('[data-toggle="tooltip"]').tooltip();
        });

        page.init = function () { };
    },
    initToggle: function () {
        $(document).on('click', 'a[x-toggle]', function (e) {
            e.preventDefault();
            var $this = $(this);
            var $container = $this.closest(".show");
            var $target = $($this.attr('href'));

            $this.trigger("x-toggling", { hidden: $container.get(0), shown: $target.get(0) });
            $container.toggleClass('hidden show');
            $target.toggleClass('hidden show');
            $this.trigger("x-toggled", { hidden: $container.get(0), shown: $target.get(0) });
        });
        page.initToggle = function () { };
    },
    initAjaxPager: function () {
        $(document).on("click", ".paged-list-pager a", function () {
            var $this = $(this);
            var url = $this.attr("href");
            if (url)
                $.ajax({
                    url: url,
                    type: 'GET',
                    cache: false,
                    success: function (result) {
                        $this.closest('.paged-list').replaceWith(result);
                    }
                });
            return false;
        });
        page.initAjaxPager = function () { };
    }
};

var utils = {
    loadImages: function (sources, callback) {
        var images = {};
        var loadedImages = 0;
        var numImages = 0;
        // get num of sources
        for (var src in sources) {
            numImages++;
        }
        for (var src in sources) {
            images[src] = new Image();
            images[src].onload = function () {
                if (++loadedImages >= numImages) {
                    callback(images);
                }
            };
            images[src].src = sources[src];
        }
    },
    resetFormElement: function (element) {
        var $element = $(element);
        $element.wrap('<form>').closest('form').get(0).reset();
        $element.unwrap();
    },
    // https://gist.github.com/dperini/729294
    validateUrlRegExp: new RegExp(
      "^" +
    // protocol identifier
        "(?:(?:https?|ftp)://)" +
    // user:pass authentication
        "(?:\\S+(?::\\S*)?@)?" +
        "(?:" +
    // IP address exclusion
    // private & local networks
          "(?!(?:10|127)(?:\\.\\d{1,3}){3})" +
          "(?!(?:169\\.254|192\\.168)(?:\\.\\d{1,3}){2})" +
          "(?!172\\.(?:1[6-9]|2\\d|3[0-1])(?:\\.\\d{1,3}){2})" +
    // IP address dotted notation octets
    // excludes loopback network 0.0.0.0
    // excludes reserved space >= 224.0.0.0
    // excludes network & broacast addresses
    // (first & last IP address of each class)
          "(?:[1-9]\\d?|1\\d\\d|2[01]\\d|22[0-3])" +
          "(?:\\.(?:1?\\d{1,2}|2[0-4]\\d|25[0-5])){2}" +
          "(?:\\.(?:[1-9]\\d?|1\\d\\d|2[0-4]\\d|25[0-4]))" +
        "|" +
    // host name
          "(?:(?:[a-z\\u00a1-\\uffff0-9]-*)*[a-z\\u00a1-\\uffff0-9]+)" +
    // domain name
          "(?:\\.(?:[a-z\\u00a1-\\uffff0-9]-*)*[a-z\\u00a1-\\uffff0-9]+)*" +
    // TLD identifier
          "(?:\\.(?:[a-z\\u00a1-\\uffff]{2,}))" +
    // TLD may end with dot
          "\\.?" +
        ")" +
    // port number
        "(?::\\d{2,5})?" +
    // resource path
        "(?:[/?#]\\S*)?" +
      "$", "i"
    ),
    validateUrl: function (value) {
        return utils.validateUrlRegExp.test(value);
    },
};

page.init();
