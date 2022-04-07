using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.UI.Helpers;

public static partial class HtmlHelperExtensions
{
    // based on: https://github.com/dncuug/X.PagedList/blob/68345177ef1de43b2538134378d9abe6292ff83d/src/X.PagedList.Web.Common/HtmlHelper.cs
    public static IHtmlContent Pager(this IHtmlHelper htmlHelper, in PagerInfo info, Func<int, string?> generatePageUrl, PagerRenderOptions options)
    {
        if (options.Display == false || options.Display == null && info.PageCount <= 1)
            return HtmlString.Empty;

        var listItemLinks = new List<TagBuilder>();

        //calculate start and end of range of page numbers
        var firstPageToDisplay = 1;
        var lastPageToDisplay = info.PageCount;
        var pageNumbersToDisplay = lastPageToDisplay;

        if (options.MaximumPageNumbersToDisplay.HasValue && info.PageCount > options.MaximumPageNumbersToDisplay)
        {
            // cannot fit all pages into pager
            var maxPageNumbersToDisplay = options.MaximumPageNumbersToDisplay.Value;

            firstPageToDisplay = info.PageNumber - maxPageNumbersToDisplay / 2;

            if (firstPageToDisplay < 1)
                firstPageToDisplay = 1;

            pageNumbersToDisplay = maxPageNumbersToDisplay;
            lastPageToDisplay = firstPageToDisplay + pageNumbersToDisplay - 1;

            if (lastPageToDisplay > info.PageCount)
                firstPageToDisplay = info.PageCount - maxPageNumbersToDisplay + 1;
        }

        //first
        if (options.DisplayLinkToFirstPage == true || options.DisplayLinkToFirstPage == null && firstPageToDisplay > 1)
            listItemLinks.Add(First(info, generatePageUrl, options));

        //previous
        if (options.DisplayLinkToPreviousPage == true || options.DisplayLinkToPreviousPage == null && !info.IsFirstPage)
            listItemLinks.Add(Previous(info, generatePageUrl, options));

        //page
        if (options.DisplayLinkToIndividualPages)
        {
            //if there are previous page numbers not displayed, show an ellipsis
            if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && firstPageToDisplay > 1)
                listItemLinks.Add(PreviousEllipsis(info, generatePageUrl, options, firstPageToDisplay));

            for (int i = firstPageToDisplay, n = firstPageToDisplay + pageNumbersToDisplay; i < n; i++)
            {
                //show delimiter between page numbers
                if (i > firstPageToDisplay && !string.IsNullOrWhiteSpace(options.DelimiterBetweenPageNumbers))
                    listItemLinks.Add(WrapInListItem(options.DelimiterBetweenPageNumbers));

                //show page number link
                listItemLinks.Add(Page(i, info, generatePageUrl, options));
            }

            //if there are subsequent page numbers not displayed, show an ellipsis
            if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && firstPageToDisplay + pageNumbersToDisplay - 1 < info.PageCount)
                listItemLinks.Add(NextEllipsis(info, generatePageUrl, options, lastPageToDisplay));
        }

        //next
        if (options.DisplayLinkToNextPage == true || options.DisplayLinkToNextPage == null && !info.IsLastPage)
            listItemLinks.Add(Next(info, generatePageUrl, options));

        //last
        if (options.DisplayLinkToLastPage == true || options.DisplayLinkToLastPage == null && lastPageToDisplay < info.PageCount)
            listItemLinks.Add(Last(info, generatePageUrl, options));

        if (listItemLinks.Count > 0)
        {
            //append class to first item in list?
            if (!string.IsNullOrWhiteSpace(options.ClassToApplyToFirstListItemInPager))
                listItemLinks[0].AddCssClass(options.ClassToApplyToFirstListItemInPager);

            //append class to last item in list?
            if (!string.IsNullOrWhiteSpace(options.ClassToApplyToLastListItemInPager))
                listItemLinks[^1].AddCssClass(options.ClassToApplyToLastListItemInPager);

            //append classes to all list item links
            if (options.LiElementClasses != null)
            {
                for (int i = 0, n = listItemLinks.Count; i < n; i++)
                {
                    foreach (var c in options.LiElementClasses)
                        listItemLinks[i].AddCssClass(c);
                }
            }
        }

        var ul = new TagBuilder("ul");

        for (int i = 0, n = listItemLinks.Count; i < n; i++)
            ul.InnerHtml.AppendHtml(listItemLinks[i]);

        if (options.UlElementClasses != null)
        {
            foreach (var c in options.UlElementClasses ?? Enumerable.Empty<string>())
                ul.AddCssClass(c);
        }

        if (options.UlElementAttributes != null)
        {
            foreach (var c in options.UlElementAttributes)
                ul.MergeAttribute(c.Key, c.Value);
        }

        return ul;
    }

    private static TagBuilder WrapInListItem(string text)
    {
        var li = new TagBuilder("li");

        li.InnerHtml.SetContent(text);

        return li;
    }

    private static TagBuilder WrapInListItem(TagBuilder inner, PagerRenderOptions options, params string[] classes)
    {
        var li = new TagBuilder("li");

        foreach (var @class in classes)
            li.AddCssClass(@class);

        li.InnerHtml.AppendHtml(inner);

        return li;
    }

    private static TagBuilder First(in PagerInfo info, Func<int, string?> generatePageUrl, PagerRenderOptions options)
    {
        const int targetPageNumber = 1;
        var first = new TagBuilder("a");

        first.InnerHtml.AppendFormat(options.LinkToFirstPageFormat, targetPageNumber);

        if (options.PageClasses != null)
        {
            foreach (var c in options.PageClasses)
                first.AddCssClass(c);
        }

        if (info.IsFirstPage)
            return WrapInListItem(first, options, "pager-skipToFirst", "disabled");

        first.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(first, options, "pager-skipToFirst");
    }

    private static TagBuilder Previous(in PagerInfo info, Func<int, string?> generatePageUrl, PagerRenderOptions options)
    {
        var targetPageNumber = info.PageNumber - 1;
        var previous = new TagBuilder("a");

        previous.InnerHtml.AppendFormat(options.LinkToPreviousPageFormat, targetPageNumber);

        previous.Attributes.Add("rel", "prev");

        if (options.PageClasses != null)
        {
            foreach (var c in options.PageClasses)
                previous.AddCssClass(c);
        }

        if (!info.HasPreviousPage)
            return WrapInListItem(previous, options, options.PreviousElementClass, "disabled");

        previous.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(previous, options, options.PreviousElementClass);
    }

    private static TagBuilder Page(int i, in PagerInfo info, Func<int, string?> generatePageUrl, PagerRenderOptions options)
    {
        var format = options.FunctionToDisplayEachPageNumber ?? (pageNumber => string.Format(options.LinkToIndividualPageFormat, pageNumber));

        var targetPageNumber = i;
        var page = i == info.PageNumber ? new TagBuilder("span") : new TagBuilder("a");

        page.InnerHtml.SetContent(format(targetPageNumber));

        if (options.PageClasses != null)
        {
            foreach (var c in options.PageClasses ?? Enumerable.Empty<string>())
                page.AddCssClass(c);
        }

        if (i == info.PageNumber)
            return WrapInListItem(page, options, options.ActiveLiElementClass);

        page.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(page, options);
    }

    private static TagBuilder Next(in PagerInfo info, Func<int, string?> generatePageUrl, PagerRenderOptions options)
    {
        var targetPageNumber = info.PageNumber + 1;
        var next = new TagBuilder("a");

        next.InnerHtml.AppendFormat(options.LinkToNextPageFormat, targetPageNumber);

        next.Attributes.Add("rel", "next");

        if (options.PageClasses != null)
        {
            foreach (var c in options.PageClasses)
                next.AddCssClass(c);
        }

        if (!info.HasNextPage)
        {
            return WrapInListItem(next, options, options.NextElementClass, "disabled");
        }

        next.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(next, options, options.NextElementClass);
    }

    private static TagBuilder Last(in PagerInfo info, Func<int, string?> generatePageUrl, PagerRenderOptions options)
    {
        var targetPageNumber = info.PageCount;
        var last = new TagBuilder("a");

        last.InnerHtml.AppendFormat(options.LinkToLastPageFormat, targetPageNumber);

        if (options.PageClasses != null)
        {
            foreach (var c in options.PageClasses)
                last.AddCssClass(c);
        }

        if (info.IsLastPage)
            return WrapInListItem(last, options, "pager-skipToLast", "disabled");

        last.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(last, options, "pager-skipToLast");
    }

    private static TagBuilder PreviousEllipsis(in PagerInfo info, Func<int, string?> generatePageUrl, PagerRenderOptions options, int firstPageToDisplay)
    {
        var previous = new TagBuilder("a");

        previous.InnerHtml.Append(options.EllipsesFormat);

        previous.Attributes.Add("rel", "prev");
        previous.AddCssClass(options.PreviousElementClass);

        if (options.EllipsesClasses != null)
        {
            foreach (var c in options.EllipsesClasses)
                previous.AddCssClass(c);
        }

        if (!info.HasPreviousPage)
            return WrapInListItem(previous, options, options.EllipsesElementClass, "disabled");

        var targetPageNumber = firstPageToDisplay - 1;

        previous.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(previous, options, options.EllipsesElementClass);
    }

    private static TagBuilder NextEllipsis(in PagerInfo info, Func<int, string?> generatePageUrl, PagerRenderOptions options, int lastPageToDisplay)
    {
        var next = new TagBuilder("a");

        next.InnerHtml.Append(options.EllipsesFormat);

        next.Attributes.Add("rel", "next");
        next.AddCssClass(options.NextElementClass);

        if (options.EllipsesClasses != null)
        {
            foreach (var c in options.EllipsesClasses)
                next.AddCssClass(c);
        }

        if (!info.HasNextPage)
            return WrapInListItem(next, options, options.EllipsesElementClass, "disabled");

        var targetPageNumber = lastPageToDisplay + 1;

        next.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(next, options, options.EllipsesElementClass);
    }
}
