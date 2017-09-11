using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Mvc.Helpers;

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
    }
}