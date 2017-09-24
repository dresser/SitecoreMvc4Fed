using System;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Mvc.Helpers;
using System.Web;
using Sitecore.Data.Fields;
using Sitecore.Foundation.FedEx.DynamicPlaceholders;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Foundation.FedEx
{
    public static class SitecoreHelperExtensions
    {
        public static ID[] SiteRootTemplateIds => ID.ParseArray(
            Settings.GetSetting("Sitecore.Foundation.FedEx.SiteRootTemplates"));

        public static ID[] HomePageTemplateIds => ID.ParseArray(
            Settings.GetSetting("Sitecore.Foundation.FedEx.HomePageTemplates"));

        public static ID[] SettingsTemplateIds => ID.ParseArray(
            Settings.GetSetting("Sitecore.Foundation.FedEx.SettingsTemplates"));

        public static ID DictionaryRootTemplateId => ID.Parse(
            Settings.GetSetting("Sitecore.Foundation.FedEx.DictionaryRootTemplate"));

        public static Item GetSiteRootItem(this SitecoreHelper sitecoreHelper)
        {
            return sitecoreHelper.CurrentItem.Axes.GetAncestors()
                .FirstOrDefault(a => SiteRootTemplateIds.Any(a.IsDerived));
        }

        public static Item GetHomePageItem(this SitecoreHelper sitecoreHelper)
        {
            return sitecoreHelper.CurrentItem.GetAncestorsAndSelf()
                .FirstOrDefault(a => HomePageTemplateIds.Any(a.IsDerived));
        }

        public static Item GetSettingsItem(this SitecoreHelper sitecoreHelper)
        {
            return GetSiteRootItem(sitecoreHelper)
                .Children
                .FirstOrDefault(c => SettingsTemplateIds.Any(c.IsDerived));
        }

        public static string GetDictionaryText(this SitecoreHelper sitecoreHelper, string dictionaryPath)
        {
            var settingsItem = GetSettingsItem(sitecoreHelper);
            var dictionary = settingsItem?.Children.FirstOrDefault(c => c.IsDerived(DictionaryRootTemplateId));
            var folderAndItem = dictionaryPath.Trim(new[] {'/'})
                .Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (folderAndItem.Length != 2)
            {
                return null;
            }
            var folder = dictionary?.Children[folderAndItem[0]];
            var item = folder?.Children[folderAndItem[1]];
            return item?["Phrase"];
        }

        public static Item[] GetBreadcrumbItems(this SitecoreHelper sitecoreHelper)
        {
            var ancestorsAndSelf = sitecoreHelper.CurrentItem.Axes.GetAncestors();
            for (var i = 0; i < ancestorsAndSelf.Length; i++)
            {
                if (HomePageTemplateIds.Any(ancestorsAndSelf[i].IsDerived))
                {
                    return ancestorsAndSelf.Skip(i).ToArray();
                }
            }
            return new Item[] { };
        }

        public static string GetItemUrl(this SitecoreHelper sitecoreHelper, Item item)
        {
            return Links.LinkManager.GetItemUrl(item);
        }

        public static string GetLinkFieldUrl(this SitecoreHelper sitecoreHelper, string fieldName)
        {
            return GetLinkFieldUrl(sitecoreHelper, sitecoreHelper.CurrentItem, fieldName);
        }

        public static string GetLinkFieldUrl(this SitecoreHelper sitecoreHelper, Item item, string fieldName)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            var fieldalue = item[fieldName];
            if (ID.IsID(item[fieldName]))
            {
                var linkItem = item.Database.GetItem(ID.Parse(fieldalue));
                return linkItem != null ? Sitecore.Links.LinkManager.GetItemUrl(linkItem) : string.Empty;
            }
            var field = item.Fields[fieldName];
            if (field == null || !(FieldTypeManager.GetField(field) is LinkField))
            {
                return string.Empty;
            }
            else
            {
                LinkField linkField = (LinkField)field;
                switch (linkField.LinkType.ToLower())
                {
                    case "internal":
                        // Use LinkMananger for internal links, if link is not empty
                        return linkField.TargetItem != null ? Sitecore.Links.LinkManager.GetItemUrl(linkField.TargetItem) : string.Empty;
                    case "media":
                        // Use MediaManager for media links, if link is not empty
                        return linkField.TargetItem != null ? Sitecore.Resources.Media.MediaManager.GetMediaUrl(linkField.TargetItem) : string.Empty;
                    case "external":
                        // Just return external links
                        return linkField.Url;
                    case "anchor":
                        // Prefix anchor link with # if link if not empty
                        return !string.IsNullOrEmpty(linkField.Anchor) ? "#" + linkField.Anchor : string.Empty;
                    case "mailto":
                        // Just return mailto link
                        return linkField.Url;
                    case "javascript":
                        // Just return javascript
                        return linkField.Url;
                    default:
                        // Just please the compiler, this
                        // condition will never be met
                        return linkField.Url;
                }
            }
        }

        public static Item GetLinkedItem(this SitecoreHelper sitecoreHelper, Item item, string fieldName)
        {
            ID id;
            if (ID.TryParse(item[fieldName], out id))
            {
                return item.Database.GetItem(id);
            }
            return null;
        }

        public static Item[] GetLinkedItems(this SitecoreHelper sitecoreHelper, string fieldName)
        {
            return GetLinkedItems(sitecoreHelper, sitecoreHelper.CurrentItem, fieldName);
        }

        public static Item[] GetLinkedItems(this SitecoreHelper sitecoreHelper, Item item, string fieldName)
        {
            var field = item.Fields[fieldName];
            if (field == null)
            {
                return null;
            }
            var multilistField = new MultilistField(field);
            return multilistField.GetItems();
        }

        public static DateTime DateTimeField(this SitecoreHelper sitecoreHelper, string fieldName)
        {
            return DateTimeField(sitecoreHelper, sitecoreHelper.CurrentItem, fieldName);
        }

        public static DateTime DateTimeField(this SitecoreHelper sitecoreHelper, Item item, string fieldName)
        {
            DateField dateField = item.Fields[fieldName];
            return dateField?.DateTime ?? DateTime.MinValue;
        }

        public static HtmlString FormattedDateTimeField(this SitecoreHelper sitecoreHelper, string fieldName,
            string format)
        {
            return FormattedDateTimeField(sitecoreHelper, sitecoreHelper.CurrentItem, fieldName, format);
        }

        public static HtmlString FormattedDateTimeField(this SitecoreHelper sitecoreHelper, Item item, string fieldName,
            string format)
        {
            if (Sitecore.Context.PageMode.IsExperienceEditor)
            {
                return sitecoreHelper.Field(fieldName, item);
            }
            return new HtmlString(DateTimeField(sitecoreHelper, item, fieldName).ToString(format));
        }

        public static string Parameter(this SitecoreHelper sitecoreHelper, string parameterName)
        {
            return sitecoreHelper?.CurrentRendering?.Parameters[parameterName];
        }

        public static HtmlString DynamicPlaceholder(this SitecoreHelper sitecoreHelper, string placeholderName)
        {
            var placeholder = PlaceholdersContext.Add(placeholderName, RenderingContext.Current.Rendering.UniqueId);
            return sitecoreHelper.Placeholder(placeholder);
        }

        public static HtmlString DynamicPlaceholder(this SitecoreHelper sitecoreHelper, string placeholderName, bool useStaticPlaceholderNames)
        {
            return useStaticPlaceholderNames ? sitecoreHelper.Placeholder(placeholderName) : DynamicPlaceholder(sitecoreHelper, placeholderName);
        }
    }
}