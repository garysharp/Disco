using Disco.Models.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Models.Shared
{
    public class FancyTreeNode
    {
        public string title { get; set; }
        public string key { get; set; }
        public bool expanded { get; set; }
        public bool folder { get; set; }
        public bool selected { get; set; }
        public bool unselectable { get; set; }
        public FancyTreeNode[] children { get; set; }
        public string tooltip { get; set; }

        public static FancyTreeNode FromClaimNavigatorItem(IClaimNavigatorItem Item, bool Unselectable)
        {
            FancyTreeNode[] children = Item.IsGroup ? Item.Children.Where(i => !i.Hidden).Select(i => FromClaimNavigatorItem(i, Unselectable)).ToArray() : null;

            return new FancyTreeNode()
            {
                key = Item.Key,
                title = Item.Name,
                folder = children != null && children.Length > 0,
                tooltip = Item.Description,
                children = children == null || children.Length == 0 ? null : children,
                selected = Item.Value.HasValue && Item.Value.Value,
                unselectable = Unselectable
            };
        }
    }
}