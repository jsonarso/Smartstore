﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Smartstore.Core.Content.Menus;
using Smartstore.Utilities;
using Smartstore.Web.TagHelpers.Shared;

namespace Smartstore.Web.Rendering.Builders
{
    public class TabFactory
    {
        public TabFactory(TabStripTagHelper tabStrip, TagHelperContext context)
        {
            Guard.NotNull(tabStrip, nameof(tabStrip));
            Guard.NotNull(context, nameof(context));

            TabStrip = tabStrip;
            Context = context;
        }

        internal TabStripTagHelper TabStrip { get; }
        internal TagHelperContext Context { get; }

        /// <summary>
        /// Adds a new tab item to the end of the current tabs collection.
        /// </summary>
        /// <param name="buildAction">Build action.</param>
        public Task AddAsync(Action<TabItemBuilder> buildAction)
        {
            return CreateTagHelper(buildAction, -1);
        }

        /// <summary>
        /// Inserts a new tab before tab with given <paramref name="tabName"/>.
        /// If the adjacent tab does not exist, the new tab will be appended 
        /// to the current tabs collection.
        /// </summary>
        /// <param name="buildAction">Build action.</param>
        public Task InsertBeforeAsync(string tabName, Action<TabItemBuilder> buildAction)
        {
            Guard.NotEmpty(tabName, nameof(tabName));

            return CreateTagHelper(
                buildAction, 
                TabStrip.Tabs.FindIndex(x => x.TabName.EqualsNoCase(tabName)));
        }

        /// <summary>
        /// Inserts a new tab after tab with given <paramref name="tabName"/>.
        /// If the adjacent tab does not exist, the new tab will be appended 
        /// to the current tabs collection.
        /// </summary>
        /// <param name="buildAction">Build action.</param>
        public Task InsertAfterAsync(string tabName, Action<TabItemBuilder> buildAction)
        {
            Guard.NotEmpty(tabName, nameof(tabName));

            var index = TabStrip.Tabs.FindIndex(x => x.TabName.EqualsNoCase(tabName));
            if (index > -1)
            {
                index = index == TabStrip.Tabs.Count - 1 ? -1 : index + 1;
            }

            return CreateTagHelper(buildAction, index);
        }

        private Task<TabTagHelper> CreateTagHelper(Action<TabItemBuilder> buildAction, int order)
        {
            Guard.NotNull(buildAction, nameof(buildAction));

            var builder = new TabItemBuilder(new TabItem(), TabStrip.HtmlHelper);
            buildAction(builder);
            return ConvertToTagHelper(builder.AsItem(), order);
        }

        private async Task<TabTagHelper> ConvertToTagHelper(TabItem item, int order)
        {
            var tagHelper = new TabTagHelper
            {
                ViewContext = TabStrip.ViewContext,
                Selected = item.Selected,
                Disabled = !item.Enabled,
                Visible = item.Visible,
                Ajax = item.Ajax,
                Title = item.Text,
                Name = item.Name,
                BadgeStyle = (BadgeStyle)item.BadgeStyle,
                BadgeText = item.BadgeText,
                Icon = item.Icon,
                ImageUrl = item.ImageUrl,
                Summary = item.Summary,
                DisplayOrder = order
            };

            if (item.IconLibrary == "bi" && tagHelper.Icon.HasValue())
            {
                tagHelper.Icon = tagHelper.Icon.EnsureStartsWith("bi:");
            }

            // Create TagHelperContext for tab passing it parent context's items dictionary (that's what Razor does)
            var context = new TagHelperContext("tab", new TagHelperAttributeList(), Context.Items, CommonHelper.GenerateRandomDigitCode(10));

            // Must init tab, otherwise "Tabs" list is empty inside tabstrip helper.
            tagHelper.Init(context);

            var outputAttrList = new TagHelperAttributeList();

            item.HtmlAttributes.CopyTo(outputAttrList);
            item.LinkHtmlAttributes.CopyTo(outputAttrList);

            if (!item.HasContent && item.HasRoute)
            {
                outputAttrList.Add("href", item.GenerateUrl(tagHelper.UrlHelper));
            }

            var output = new TagHelperOutput("tab", outputAttrList, async (_, _) =>
            {
                var content = item.HasContent
                    ? await item.GetContentAsync(tagHelper.ViewContext)
                    : HtmlString.Empty;

                var contentTag = new TagBuilder("div");
                contentTag.MergeAttributes(item.ContentHtmlAttributes, false);

                TagHelperContent tabContent = new DefaultTagHelperContent()
                    .AppendHtml(contentTag.Attributes.Count > 0 ? contentTag.RenderStartTag() : HtmlString.Empty)
                    .AppendHtml(content)
                    .AppendHtml(contentTag.Attributes.Count > 0 ? contentTag.RenderEndTag() : HtmlString.Empty);

                return tabContent;
            });

            // Process tab
            await tagHelper.ProcessAsync(context, output);

            return tagHelper;
        }
    }
}
