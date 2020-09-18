/**
 * --------------------------------------------------------------------------
 * This is a modified version of the following CoreUI v3.3.0 source file(s):
 * • https://github.com/coreui/coreui/blob/master/js/dist/class-toggler.js
 * • https://github.com/coreui/coreui/blob/master/js/dist/sidebar.js
 * Licensed under MIT (https://github.com/coreui/coreui/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

(function (global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory(require('jquery'), require('bootstrap')) :
  typeof define === 'function' && define.amd ? define(['jquery', 'bootstrap'], factory) :
  (global = global || self, global.coreui = factory(global.jQuery, global.bootstrap));
}(this, (function ($, bootstrap) { 'use strict';

  $ = $ && Object.prototype.hasOwnProperty.call($, 'default') ? $['default'] : $;
  bootstrap = bootstrap && Object.prototype.hasOwnProperty.call(bootstrap, 'default') ? bootstrap['default'] : bootstrap;

  function _defineProperties(target, props) {
    for (var i = 0; i < props.length; i++) {
      var descriptor = props[i];
      descriptor.enumerable = descriptor.enumerable || false;
      descriptor.configurable = true;
      if ("value" in descriptor) descriptor.writable = true;
      Object.defineProperty(target, descriptor.key, descriptor);
    }
  }

  function _createClass(Constructor, protoProps, staticProps) {
    if (protoProps) _defineProperties(Constructor.prototype, protoProps);
    if (staticProps) _defineProperties(Constructor, staticProps);
    return Constructor;
  }

  /**
   * --------------------------------------------------------------------------
   * Bootstrap (v5.0.0-alpha1): util/index.js
   * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
   * --------------------------------------------------------------------------
   */
  var TRANSITION_END = bootstrap.Util.TRANSITION_END;

  var reflow = bootstrap.Util.reflow;

  /**** Class Toggler ****/

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME$4 = 'class-toggler';
  var VERSION$4 = '3.2.0';
  var DATA_KEY$4 = 'coreui.class-toggler';
  var EVENT_KEY$4 = "." + DATA_KEY$4;
  var DATA_API_KEY$4 = '.data-api';
  var Default$2 = {
    breakpoints: '-sm,-md,-lg,-xl',
    postfix: '-show',
    responsive: false,
    target: 'body'
  };
  var CLASS_NAME_CLASS_TOGGLER = 'c-class-toggler';
  var EVENT_CLASS_TOGGLE = 'classtoggle';
  var EVENT_CLICK_DATA_API$4 = "click" + EVENT_KEY$4 + DATA_API_KEY$4;
  var SELECTOR_CLASS_TOGGLER = '.c-class-toggler';

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var ClassToggler = /*#__PURE__*/function () {
    function ClassToggler($element) {
      this._$element = $element;

      $element.data(DATA_KEY$4, this);
    } // Getters


    var _proto = ClassToggler.prototype;

    // Public
    _proto.toggle = function toggle() {
      var _this = this;

      this._getElementDataAttributes(this._$element).forEach(function (dataAttributes) {
        var $element;
        var target = dataAttributes.target,
            toggle = dataAttributes.toggle;

        if (target === '_parent' || target === 'parent') {
          $element = _this._$element.parent();
        } else {
          $element = $(target).first();
        }

        toggle.forEach(function (object) {
          var className = object.className,
              responsive = object.responsive,
              postfix = object.postfix;
          var breakpoints = typeof object.breakpoints === 'undefined' || object.breakpoints === null ? null : _this._arrayFromString(object.breakpoints); // eslint-disable-next-line no-negated-condition

          if (!responsive) {
            $element.toggleClass(className);
            var add = $element.hasClass(className);

            $element.trigger(EVENT_CLASS_TOGGLE, {
              target: target,
              add: add,
              className: className
            });
          } else {
            var currentBreakpointIndex = -1;
            breakpoints.forEach(function (breakpoint, i) {
              if (className.indexOf(breakpoint) >= 0) {
                currentBreakpointIndex = i;
              }
            });

            var responsiveClassNames = [];

            if (currentBreakpointIndex < 0) {
              responsiveClassNames.push(className);
            } else {
              var currentBreakpoint = breakpoints[currentBreakpointIndex];
              responsiveClassNames.push(className.replace("" + currentBreakpoint + postfix, postfix));
              breakpoints.splice(0, currentBreakpointIndex + 1).forEach(function (breakpoint) {
                responsiveClassNames.push(className.replace("" + currentBreakpoint + postfix, "" + breakpoint + postfix));
              });
            }

            var addResponsiveClasses = false;
            responsiveClassNames.forEach(function (responsiveClassName) {
              if ($element.hasClass(responsiveClassName)) {
                addResponsiveClasses = true;
              }
            });

            if (addResponsiveClasses) {
              responsiveClassNames.forEach(function (responsiveClassName) {
                $element.removeClass(responsiveClassName);

                $element.trigger(EVENT_CLASS_TOGGLE, {
                  target: target,
                  add: false,
                  className: responsiveClassName
                });
              });
            } else {
              $element.addClass(className);

              $element.trigger(EVENT_CLASS_TOGGLE, {
                target: target,
                add: true,
                className: className
              });
            }
          }
        });
      });
    } // Private
    ;

    _proto._arrayFromString = function _arrayFromString(string) {
      return string.replace(/ /g, '').split(',');
    };

    _proto._getDataAttributes = function _getDataAttributes(value) {
      try {
        return JSON.parse(value.replace(/'/g, '"'));
      }
      catch (_unused) {
        return value;
      }
    };

    _proto._getToggleDetails = function _getToggleDetails(classNames, responsive, breakpoints, postfix) {
      var _this2 = this;

      var ToggleDetails = // eslint-disable-next-line default-param-last
      function ToggleDetails(className, responsive, breakpoints, postfix) {
        if (responsive === void 0) {
          responsive = Default$2.responsive;
        }

        this.className = className;
        this.responsive = responsive;
        this.breakpoints = breakpoints;
        this.postfix = postfix;
      };

      var toggle = [];

      if (Array.isArray(classNames)) {
        classNames.forEach(function (className, index) {
          var responsiveValue = _this2._ifArray(responsive, index);
          var breakpointsValue = responsiveValue ? _this2._ifArray(breakpoints, index) : null;
          var postfixValue = responsiveValue ? _this2._ifArray(postfix, index) : null;
          toggle.push(new ToggleDetails(className, responsiveValue, breakpointsValue, postfixValue));
        });
      } else {
        breakpoints = responsive ? breakpoints : null;
        postfix = responsive ? postfix : null;
        toggle.push(new ToggleDetails(classNames, responsive, breakpoints, postfix));
      }

      return toggle;
    };

    _proto._ifArray = function _ifArray(array, index) {
      return Array.isArray(array) ? array[index] : array;
    };

    _proto._getElementDataAttributes = function _getElementDataAttributes($element) {
      var _this2 = this;

      var value;
      var targets = typeof (value = $element.attr('data-target')) === 'undefined' ? Default$2.target : this._getDataAttributes(value);
      var classNames = typeof (value = $element.attr('data-class')) === 'undefined' ? 'undefined' : this._getDataAttributes(value);
      var responsive = typeof (value = $element.attr('data-responsive')) ? Default$2.responsive : this._getDataAttributes(value);
      var breakpoints = typeof (value = $element.attr('data-breakpoints')) ? Default$2.breakpoints : this._getDataAttributes(value);
      var postfix = typeof (value = $element.attr('data-postfix')) ? Default$2.postfix : this._getDataAttributes(value);
      var toggle = [];

      var TargetDetails = function TargetDetails(target, toggle) {
        this.target = target;
        this.toggle = toggle;
      };

      if (Array.isArray(targets)) {
        targets.forEach(function (target, index) {
          toggle.push(new TargetDetails(target, _this2._getToggleDetails(_this2._ifArray(classNames, index), _this2._ifArray(responsive, index), _this2._ifArray(breakpoints, index), _this2._ifArray(postfix, index))));
        });
      } else {
        toggle.push(new TargetDetails(targets, this._getToggleDetails(classNames, responsive, breakpoints, postfix)));
      }

      return toggle;
    } // Static
    ;

    ClassToggler._classTogglerInterface = function _classTogglerInterface($element, config) {
      var data = $element.data(DATA_KEY$4);

      var _config = typeof config === 'object' && config;

      if (!data) {
        data = new ClassToggler($element, _config);
      }

      if (typeof config === 'string') {
        if (typeof data[config] === 'undefined') {
          throw new TypeError("No method named \"" + config + "\"");
        }

        data[config]();
      }
    };

    ClassToggler.jQueryInterface = function jQueryInterface(config) {
      return this.each(function () {
        ClassToggler._classTogglerInterface($(this), config);
      });
    };

    _createClass(ClassToggler, null, [{
      key: "VERSION",
      get: function get() {
        return VERSION$4;
      }
    }]);

    return ClassToggler;
  }();

  /**
   * ------------------------------------------------------------------------
   * Data Api implementation
   * ------------------------------------------------------------------------
   */

  $(document).on(EVENT_CLICK_DATA_API$4, SELECTOR_CLASS_TOGGLER, function (event) {
    event.preventDefault();
    var $toggler = $(event.target);

    if (!$toggler.hasClass(CLASS_NAME_CLASS_TOGGLER)) {
      $toggler = $toggler.closest(SELECTOR_CLASS_TOGGLER);
    }

    ClassToggler._classTogglerInterface($toggler, 'toggle');
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   * add .c-class-toggler to jQuery only if jQuery is present
   */

  var JQUERY_NO_CONFLICT$4 = $.fn[NAME$4];
  $.fn[NAME$4] = ClassToggler.jQueryInterface;
  $.fn[NAME$4].Constructor = ClassToggler;

  $.fn[NAME$4].noConflict = function () {
    $.fn[NAME$4] = JQUERY_NO_CONFLICT$4;
    return ClassToggler.jQueryInterface;
  };

  /**** Sidebar ****/

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME$b = 'sidebar';
  var VERSION$b = '3.2.0';
  var DATA_KEY$b = 'coreui.sidebar';
  var EVENT_KEY$b = "." + DATA_KEY$b;
  var DATA_API_KEY$9 = '.data-api';
  var Default$9 = {
    breakpoints: {
      xs: 'c-sidebar-show',
      sm: 'c-sidebar-sm-show',
      md: 'c-sidebar-md-show',
      lg: 'c-sidebar-lg-show',
      xl: 'c-sidebar-xl-show'
    },
    dropdownAccordion: true
  };
  var DefaultType$7 = {
    breakpoints: 'object',
    dropdownAccordion: '(string|boolean)'
  };
  var CLASS_NAME_ACTIVE$4 = 'c-active';
  var CLASS_NAME_$backdrop$1 = 'c-sidebar-backdrop';
  var CLASS_NAME_FADE$3 = 'c-fade';
  var CLASS_NAME_NAV_DROPDOWN = 'c-sidebar-nav-dropdown';
  var CLASS_NAME_NAV_DROPDOWN_TOGGLE$1 = 'c-sidebar-nav-dropdown-toggle';
  var CLASS_NAME_SHOW$6 = 'c-show';
  var CLASS_NAME_SIDEBAR_MINIMIZED = 'c-sidebar-minimized';
  var CLASS_NAME_SIDEBAR_OVERLAID = 'c-sidebar-overlaid';
  var CLASS_NAME_SIDEBAR_UNFOLDABLE = 'c-sidebar-unfoldable';
  var EVENT_CLASS_TOGGLE$1 = 'classtoggle';
  var EVENT_CLICK_DATA_API$8 = "click" + EVENT_KEY$b + DATA_API_KEY$9;
  var EVENT_CLOSE$1 = "close" + EVENT_KEY$b;
  var EVENT_CLOSED$1 = "closed" + EVENT_KEY$b;
  var EVENT_OPEN = "open" + EVENT_KEY$b;
  var EVENT_OPENED = "opened" + EVENT_KEY$b;
  var SELECTOR_NAV_DROPDOWN_TOGGLE = '.c-sidebar-nav-dropdown-toggle';
  var SELECTOR_NAV_DROPDOWN$1 = '.c-sidebar-nav-dropdown';
  var SELECTOR_NAV_LINK$1 = '.c-sidebar-nav-link';
  var SELECTOR_NAVIGATION_CONTAINER = '.c-sidebar-nav';
  var SELECTOR_SIDEBAR = '.c-sidebar';

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Sidebar = /*#__PURE__*/function () {
    function Sidebar($element, config) {
      this._$element = $element;
      this._open = this._isVisible();
      this._mobile = this._isMobile();
      this._overlaid = this._isOverlaid();
      this._minimize = this._isMinimized();
      this._unfoldable = this._isUnfoldable();

      //this._setActiveLink();

      this._$backdrop = null;

      this._addEventListeners();

      $element.data(DATA_KEY$b, this);
    } // Getters


    var _proto = Sidebar.prototype;

    // Public
    _proto.open = function open(breakpoint) {
      var _this = this;

      this._$element.trigger(EVENT_OPEN);

      if (this._isMobile()) {
        this._$element.addClass(this._firstBreakpointClassName());

        this._showBackdrop();

        this._$element.one(TRANSITION_END, function () {
          _this._addClickOutListener();
        });
      } else if (breakpoint) {
        this._$element.addClass(this._getBreakpointClassName(breakpoint));

        if (this._isOverlaid()) {
          this._$element.one(TRANSITION_END, function () {
            _this._addClickOutListener();
          });
        }
      } else {
        this._$element.addClass(this._firstBreakpointClassName());

        if (this._isOverlaid()) {
          this._$element.one(TRANSITION_END, function () {
            _this._addClickOutListener();
          });
        }
      }

      var complete = function complete() {
        if (_this._isVisible() === true) {
          _this._open = true;
          _this._$element.trigger(EVENT_OPENED);
        }
      };

      this._$element.one(TRANSITION_END, complete);
    };

    _proto.close = function close(breakpoint) {
      var _this2 = this;

      this._$element.trigger(EVENT_CLOSE$1);

      if (this._isMobile()) {
        this._$element.removeClass(this._firstBreakpointClassName());

        this._removeBackdrop();

        this._removeClickOutListener();
      } else if (breakpoint) {
        this._$element.removeClass(this._getBreakpointClassName(breakpoint));

        if (this._isOverlaid()) {
          this._removeClickOutListener();
        }
      } else {
        this._$element.removeClass(this._firstBreakpointClassName());

        if (this._isOverlaid()) {
          this._removeClickOutListener();
        }
      }

      var complete = function complete() {
        if (_this2._isVisible() === false) {
          _this2._open = false;
          _this2._$element.trigger(EVENT_CLOSED$1);
        }
      };

      this._$element.one(TRANSITION_END, complete);
    };

    _proto.toggle = function toggle(breakpoint) {
      if (this._open) {
        this.close(breakpoint);
      } else {
        this.open(breakpoint);
      }
    };

    _proto.minimize = function minimize() {
      if (!this._isMobile()) {
        this._$element.addClass(CLASS_NAME_SIDEBAR_MINIMIZED);

        this._minimize = true;
      }
    };

    _proto.unfoldable = function unfoldable() {
      if (!this._isMobile()) {
        this._$element.addClass(CLASS_NAME_SIDEBAR_UNFOLDABLE);

        this._unfoldable = true;
      }
    };

    _proto._revertMinimize = function _revertMinimize() {
      var _this = this;

      this._minimize = false;
    }

    _proto._revertUnfoldable = function _revertUnfoldable() {
      this._unfoldable = false;
    }

    _proto.reset = function reset() {
      if (this._$element.hasClass(CLASS_NAME_SIDEBAR_MINIMIZED)) {
        this._$element.removeClass(CLASS_NAME_SIDEBAR_MINIMIZED);
        this._revertMinimize();
      }

      if (this._$element.hasClass(CLASS_NAME_SIDEBAR_UNFOLDABLE)) {
        this._$element.removeClass(CLASS_NAME_SIDEBAR_UNFOLDABLE);
        this._revertUnfoldable();
      }
    } // Private
    ;

    _proto._isMobile = function _isMobile() {
      return Boolean(window.getComputedStyle(this._$element[0], null).getPropertyValue('--is-mobile'));
    };

    _proto._isIOS = function _isIOS() {
      var iOSDevices = ['iPad Simulator', 'iPhone Simulator', 'iPod Simulator', 'iPad', 'iPhone', 'iPod'];
      var platform = Boolean(navigator.platform);

      if (platform) {
        while (iOSDevices.length) {
          if (navigator.platform === iOSDevices.pop()) {
            return true;
          }
        }
      }

      return false;
    };

    _proto._isMinimized = function _isMinimized() {
      return this._$element.hasClass(CLASS_NAME_SIDEBAR_MINIMIZED);
    };

    _proto._isOverlaid = function _isOverlaid() {
      return this._$element.hasClass(CLASS_NAME_SIDEBAR_OVERLAID);
    };

    _proto._isUnfoldable = function _isUnfoldable() {
      return this._$element.hasClass(CLASS_NAME_SIDEBAR_UNFOLDABLE);
    };

    _proto._isVisible = function _isVisible() {
      var rect = this._$element[0].getBoundingClientRect();
      var $window = $(window);
      return rect.top >= 0 && rect.left >= 0 && rect.bottom <= $window.height() && rect.right <= $window.width();
    };

    _proto._firstBreakpointClassName = function _firstBreakpointClassName() {
      return Object.keys(Default$9.breakpoints).map(function (key) {
        return Default$9.breakpoints[key];
      })[0];
    };

    _proto._getBreakpointClassName = function _getBreakpointClassName(breakpoint) {
      return Default$9.breakpoints[breakpoint];
    };

    _proto._removeBackdrop = function _removeBackdrop() {
      if (this._$backdrop) {
        this._$backdrop.remove();

        this._$backdrop = null;
      }
    };

    _proto._showBackdrop = function _showBackdrop() {
      if (!this._$backdrop) {
        this._$backdrop = $('<div>');
        this._$backdrop.addClass([CLASS_NAME_$backdrop$1, CLASS_NAME_FADE$3]);

        $('body').append(this._$backdrop);
        reflow(this._$backdrop);

        this._$backdrop.addClass(CLASS_NAME_SHOW$6);
      }
    };

    _proto._clickOutListener = function _clickOutListener(event, sidebar) {
      if (!$(event.target).closest(SELECTOR_SIDEBAR).length) {
        // or use:
        event.preventDefault();
        event.stopPropagation();
        sidebar.close();
      }
    };

    _proto._addClickOutListener = function _addClickOutListener() {
      var _this3 = this;

      $(document).on(EVENT_CLICK_DATA_API$8, function (event) {
        _this3._clickOutListener(event, _this3);
      });
    };

    _proto._removeClickOutListener = function _removeClickOutListener() {
      $(document).off(EVENT_CLICK_DATA_API$8);
    } // Sidebar navigation
    ;

    _proto._toggleDropdown = function _toggleDropdown($toggler) {
      if (!$toggler.hasClass(CLASS_NAME_NAV_DROPDOWN_TOGGLE$1)) {
        $toggler = $toggler.closest(SELECTOR_NAV_DROPDOWN_TOGGLE);
      }

      var $nav = $toggler.closest(SELECTOR_NAVIGATION_CONTAINER);

      var value;
      var dropdownAccordion = typeof (value = $nav.attr('data-dropdown-accordion')) === 'undefined' ? Default$9.dropdownAccordion : (value === 'true');

      var $togglerParent = $toggler.parent();
      if (dropdownAccordion) {
        $togglerParent.siblings().each(function () {
          var $element = $(this);
          if (!$element.is($togglerParent)) {
            if ($element.hasClass(CLASS_NAME_NAV_DROPDOWN)) {
              $element.removeClass(CLASS_NAME_SHOW$6);
            }
          }
        });
      }

      $togglerParent.toggleClass(CLASS_NAME_SHOW$6); // TODO: Set the toggler's position near to cursor after the click.
      // TODO: add transition end
    }
    ;

    //_proto._setActiveLink = function _setActiveLink() {
    //  var _this4 = this;

    //  this._$element.find(SELECTOR_NAV_LINK$1).each(function () {
    //    var $element = $(this);

    //    var currentUrl;
    //    var urlHasParams = /\\?.*=/;
    //    var urlHasQueryString = /\\?./;
    //    var urlHasHash = /#./;

    //    if (urlHasParams.test(String(window.location)) || urlHasQueryString.test(String(window.location))) {
    //      currentUrl = String(window.location).split('?')[0];
    //    } else if (urlHasHash.test(String(window.location))) {
    //      currentUrl = String(window.location).split('#')[0];
    //    } else {
    //      currentUrl = String(window.location);
    //    }

    //    if (currentUrl.slice(-1) === '#') {
    //      currentUrl = currentUrl.slice(0, -1);
    //    }

    //    if ($element.attr('href') === currentUrl) {
    //      $element.addClass(CLASS_NAME_ACTIVE$4); // eslint-disable-next-line unicorn/prefer-spread

    //      var parents = [$element[0]];
    //      parents.push.apply(parents, $element.parents().toArray());
    //      $(parents).filter(SELECTOR_NAV_DROPDOWN$1).each(function () {
    //        $(this).addClass(CLASS_NAME_SHOW$6);
    //      });
    //    }
    //  });
    //};

    _proto._addEventListeners = function _addEventListeners() {
      var _this5 = this;

      if (this._mobile && this._open) {
        this._addClickOutListener();
      }

      if (this._overlaid && this._open) {
        this._addClickOutListener();
      }

      this._$element.on(EVENT_CLASS_TOGGLE$1, function (_, eventDetail) {
        if (eventDetail.className === CLASS_NAME_SIDEBAR_MINIMIZED) {
          if (_this5._$element.hasClass(CLASS_NAME_SIDEBAR_MINIMIZED)) {
            _this5.minimize();
          } else {
            _this5._revertMinimize();
          }
        }
        else if (eventDetail.className === CLASS_NAME_SIDEBAR_UNFOLDABLE) {
          if (_this5._$element.hasClass(CLASS_NAME_SIDEBAR_UNFOLDABLE)) {
            _this5.unfoldable();
          } else {
            _this5._revertUnfoldable();
          }
        }

        var breakpointKeys = Object.keys(Default$9.breakpoints);
        var breakpointKeyIndex = -1;
        for (var i = 0; i < breakpointKeys.length; i++) {
          if (Default$9.breakpoints[breakpointKeys[i]] === eventDetail.className) {
            breakpointKeyIndex = i;
            break;
          }
        }

        if (breakpointKeyIndex >= 0) {
          var breakpoint = Default$9.breakpoints[breakpointKeys[breakpointKeyIndex]];

          if (eventDetail.add) {
            _this5.open(breakpoint);
          } else {
            _this5.close(breakpoint);
          }
        }
      });
      this._$element.on(EVENT_CLICK_DATA_API$8, SELECTOR_NAV_DROPDOWN_TOGGLE, function (event) {
        event.preventDefault();

        _this5._toggleDropdown($(event.target));
      });
      this._$element.on(EVENT_CLICK_DATA_API$8, SELECTOR_NAV_LINK$1, function () {
        if (_this5._isMobile()) {
          _this5.close();
        }
      });
    } // Static
    ;

    Sidebar._sidebarInterface = function _sidebarInterface($element, config) {
      var data = $element.data(DATA_KEY$b);

      var _config = typeof config === 'object' && config;

      if (!data) {
        data = new Sidebar($element, _config);
      }

      if (typeof config === 'string') {
        if (typeof data[config] === 'undefined') {
          throw new TypeError("No method named \"" + config + "\"");
        }

        data[config]();
      }
    };

    Sidebar.jQueryInterface = function jQueryInterface(config) {
      return this.each(function () {
        Sidebar._sidebarInterface($(this), config);
      });
    };

    Sidebar.getInstance = function getInstance(element) {
      return $(element).data(DATA_KEY$b);
    };

    _createClass(Sidebar, null, [{
      key: "VERSION",
      get: function get() {
        return VERSION$b;
      }
    }, {
      key: "Default",
      get: function get() {
        return Default$9;
      }
    }, {
      key: "DefaultType",
      get: function get() {
        return DefaultType$7;
      }
    }]);

    return Sidebar;
  }();

  /**
   * ------------------------------------------------------------------------
   * Data Api implementation
   * ------------------------------------------------------------------------
   */

  $(document).ready(function () {
    $(SELECTOR_SIDEBAR).each(function () {
      Sidebar._sidebarInterface($(this));
    });
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  var JQUERY_NO_CONFLICT$b = $.fn[NAME$b];
  $.fn[NAME$b] = Sidebar.jQueryInterface;
  $.fn[NAME$b].Constructor = Sidebar;

  $.fn[NAME$b].noConflict = function () {
    $.fn[NAME$b] = JQUERY_NO_CONFLICT$b;
    return Sidebar.jQueryInterface;
  };

  /**
   * --------------------------------------------------------------------------
   * CoreUI (v3.2.0): index.umd.js
   * Licensed under MIT (https://coreui.io/license)
   * --------------------------------------------------------------------------
   */
  var index_umd = {
    ClassToggler: ClassToggler,
    Sidebar: Sidebar,
  };

  return index_umd;

})));
