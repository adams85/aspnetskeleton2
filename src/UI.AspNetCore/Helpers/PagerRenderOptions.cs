using System;
using System.Collections.Generic;

namespace WebApp.UI.Helpers
{
    // https://github.com/dncuug/X.PagedList/blob/68345177ef1de43b2538134378d9abe6292ff83d/src/X.PagedList.Web.Common/PagedListRenderOptions.cs
    public sealed class PagerRenderOptions
    {
        public static readonly PagerRenderOptions Default = new PagerRenderOptions
        {
            Display = true,
            MaximumPageNumbersToDisplay = 5
        };

        ///<summary>
        /// The default settings render all navigation links and no descriptive text
        ///</summary>
        public PagerRenderOptions()
        {
            DisplayLinkToFirstPage = true;
            DisplayLinkToLastPage = true;
            DisplayLinkToPreviousPage = true;
            DisplayLinkToNextPage = true;
            DisplayLinkToIndividualPages = true;
            MaximumPageNumbersToDisplay = 10;
            DisplayEllipsesWhenNotShowingAllPageNumbers = true;
            EllipsesFormat = "\x2026";
            LinkToFirstPageFormat = "<<";
            LinkToPreviousPageFormat = "<";
            LinkToIndividualPageFormat = "{0}";
            LinkToNextPageFormat = ">";
            LinkToLastPageFormat = ">>";
            FunctionToDisplayEachPageNumber = null;
            ClassToApplyToFirstListItemInPager = null;
            ClassToApplyToLastListItemInPager = null;
            UlElementClasses = new[] { "pagination" };
            LiElementClasses = new[] { "page-item" };
            PageClasses = EllipsesClasses = new[] { "page-link" };
            UlElementAttributes = null;
            ActiveLiElementClass = "active";
            EllipsesElementClass = "pager-ellipses";
            PreviousElementClass = "pager-skipToPrevious";
            NextElementClass = "pager-skipToNext";
        }

        ///<summary>
        /// CSSClasses to append to the &lt;ul&gt; element in the paging control.
        ///</summary>
        public IEnumerable<string>? UlElementClasses { get; set; }

        /// <summary>
        /// Attrinutes to appendto the &lt;ul&gt; element in the paging control
        /// </summary>
        public IDictionary<string, string>? UlElementAttributes { get; set; }

        ///<summary>
        /// CSS Classes to append to every &lt;li&gt; element in the paging control.
        ///</summary>
        public IEnumerable<string>? LiElementClasses { get; set; }

        /// <summary>
        /// CSS Classes to appent to active &lt;li&gt; element in the paging control.
        /// </summary>
        public string ActiveLiElementClass { get; set; }

        ///<summary>
        /// CSS Classes to append to every &lt;a&gt; or &lt;span&gt; element that represent each page in the paging control.
        ///</summary>
        public IEnumerable<string>? PageClasses { get; set; }

        ///<summary>
        /// CSS Classes to append to previous element in the paging control.
        ///</summary>
        public string PreviousElementClass { get; set; }

        ///<summary>
        /// CSS Classes to append to next element in the paging control.
        ///</summary>
        public string NextElementClass { get; set; }

        ///<summary>
        /// CSS Classes to append to every &lt;a&gt; or &lt;span&gt; Ellipses element.
        ///</summary>
        public IEnumerable<string>? EllipsesClasses { get; set; }

        ///<summary>
        /// CSS Classes to append to Ellipses element in the paging control.
        ///</summary>
        public string EllipsesElementClass { get; set; }

        ///<summary>
        /// Specifies a CSS class to append to the first list item in the pager. If null or whitespace is defined, no additional class is added to first list item in list.
        ///</summary>
        public string? ClassToApplyToFirstListItemInPager { get; set; }

        ///<summary>
        /// Specifies a CSS class to append to the last list item in the pager. If null or whitespace is defined, no additional class is added to last list item in list.
        ///</summary>
        public string? ClassToApplyToLastListItemInPager { get; set; }

        /// <summary>
        /// If set to <see langword="true" />, always renders the paging control. If set to <see langword="null" />, render the paging control when there is more than one page.
        /// </summary>
        public bool? Display { get; set; }

        ///<summary>
        /// If set to <see langword="true" />, render a hyperlink to the first page in the list. If set to <see langword="null" />, render the hyperlink only when the first page isn't visible in the paging control.
        ///</summary>
        public bool? DisplayLinkToFirstPage { get; set; }

        ///<summary>
        /// If set to <see langword="true" />, render a hyperlink to the last page in the list. If set to <see langword="null" />, render the hyperlink only when the last page isn't visible in the paging control.
        ///</summary>
        public bool? DisplayLinkToLastPage { get; set; }

        ///<summary>
        /// If set to <see langword="true" />, render a hyperlink to the previous page of the list. If set to <see langword="null" />, render the hyperlink only when there is a previous page in the list.
        ///</summary>
        public bool? DisplayLinkToPreviousPage { get; set; }

        ///<summary>
        /// If set to <see langword="true" />, render a hyperlink to the next page of the list. If set to <see langword="null" />, render the hyperlink only when there is a next page in the list.
        ///</summary>
        public bool? DisplayLinkToNextPage { get; set; }

        ///<summary>
        /// When true, includes hyperlinks for each page in the list.
        ///</summary>
        public bool DisplayLinkToIndividualPages { get; set; }

        ///<summary>
        /// The maximum number of page numbers to display. Null displays all page numbers.
        ///</summary>
        public int? MaximumPageNumbersToDisplay { get; set; }

        ///<summary>
        /// If true, adds an ellipsis where not all page numbers are being displayed.
        ///</summary>
        ///<example>
        /// "1 2 3 4 5 ...",
        /// "... 6 7 8 9 10 ...",
        /// "... 11 12 13 14 15"
        ///</example>
        public bool DisplayEllipsesWhenNotShowingAllPageNumbers { get; set; }

        ///<summary>
        /// The pre-formatted text to display when not all page numbers are displayed at once.
        ///</summary>
        ///<example>
        /// "..."
        ///</example>
        public string EllipsesFormat { get; set; }

        ///<summary>
        /// The pre-formatted text to display inside the hyperlink to the first page. The one-based index of the page (always 1 in this case) is passed into the formatting function - use {0} to reference it.
        ///</summary>
        ///<example>
        /// "&lt;&lt; First"
        ///</example>
        public string LinkToFirstPageFormat { get; set; }

        ///<summary>
        /// The pre-formatted text to display inside the hyperlink to the previous page. The one-based index of the page is passed into the formatting function - use {0} to reference it.
        ///</summary>
        ///<example>
        /// "&lt; Previous"
        ///</example>
        public string LinkToPreviousPageFormat { get; set; }

        ///<summary>
        /// The pre-formatted text to display inside the hyperlink to each individual page. The one-based index of the page is passed into the formatting function - use {0} to reference it.
        ///</summary>
        ///<example>
        /// "{0}"
        ///</example>
        public string LinkToIndividualPageFormat { get; set; }

        ///<summary>
        /// The pre-formatted text to display inside the hyperlink to the next page. The one-based index of the page is passed into the formatting function - use {0} to reference it.
        ///</summary>
        ///<example>
        /// "Next &gt;"
        ///</example>
        public string LinkToNextPageFormat { get; set; }

        ///<summary>
        /// The pre-formatted text to display inside the hyperlink to the last page. The one-based index of the page is passed into the formatting function - use {0} to reference it.
        ///</summary>
        ///<example>
        /// "Last &gt;&gt;"
        ///</example>
        public string LinkToLastPageFormat { get; set; }

        /// <summary>
        /// A function that will render each page number when specified (and DisplayLinkToIndividualPages is true). If no function is specified, the LinkToIndividualPageFormat value will be used instead.
        /// </summary>
        public Func<int, string>? FunctionToDisplayEachPageNumber { get; set; }

        /// <summary>
        /// Text that will appear between each page number. If null or whitespace is specified, no delimiter will be shown.
        /// </summary>
        public string? DelimiterBetweenPageNumbers { get; set; }
    }
}
