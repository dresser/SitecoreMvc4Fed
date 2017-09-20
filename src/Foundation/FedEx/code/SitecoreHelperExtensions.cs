using System;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Mvc.Helpers;
using System.Collections.Generic;
using Sitecore.Data.Fields;

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
    }
}