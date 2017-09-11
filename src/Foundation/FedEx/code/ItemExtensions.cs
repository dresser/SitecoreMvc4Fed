﻿using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;

namespace Sitecore.Foundation.FedEx
{
    public static class ItemExtensions
    {
        public static bool IsDerived(this Item item, ID templateId)
        {
            if (item == null)
            {
                return false;
            }
            return !templateId.IsNull && item.IsDerived(item.Database.Templates[templateId]);
        }

        private static bool IsDerived(this Item item, Item templateItem)
        {
            if (item == null)
            {
                return false;
            }

            if (templateItem == null)
            {
                return false;
            }

            var itemTemplate = TemplateManager.GetTemplate(item);
            return itemTemplate != null && (itemTemplate.ID == templateItem.ID || itemTemplate.DescendsFrom(templateItem.ID));
        }

        public static Item[] GetAncestorsAndSelf(this Item item)
        {
            return new List<Item>(item.Axes.GetAncestors()) {item}.ToArray();
        }
    }
}