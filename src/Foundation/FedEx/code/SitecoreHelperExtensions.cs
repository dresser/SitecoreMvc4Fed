using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Mvc.Helpers;
using System.Collections.Generic;

namespace Sitecore.Foundation.FedEx
{
    public static class SitecoreHelperExtensions
    {
        public static ID[] SiteRootTemplateIds => ID.ParseArray(
            Settings.GetSetting("Sitecore.Foundation.FedEx.SiteRootTemplates"));

        public static ID[] HomePageTemplateIds => ID.ParseArray(
            Settings.GetSetting("Sitecore.Foundation.FedEx.HomePageTemplates"));

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
    }
}