﻿using Disco.Models.Services.Authorization;
using Disco.Services.Authorization.Roles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Authorization
{
    public class ClaimNavigatorItem : IClaimNavigatorItem
    {
        private Func<RoleClaims, bool> accessor { get; set; }
        
        public string Key { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool Hidden { get; private set; }

        public List<IClaimNavigatorItem> Children { get; private set; }
        public bool IsGroup { get { return Children != null; } }

        public bool? Value { get; private set; }

        internal ClaimNavigatorItem(string Key, bool Hidden)
        {
            this.Key = Key;
            var details = Claims.GetClaimDetails(Key);
            Name = details.Item1;
            Description = details.Item2;
            accessor = Claims.GetClaimAccessor(Key);
            this.Hidden = Hidden;
        }

        internal ClaimNavigatorItem(string Key, string Name, string Description, bool Hidden, List<IClaimNavigatorItem> Children)
        {
            this.Key = Key;
            this.Name = Name;
            this.Description = Description;

            this.Hidden = Hidden;

            this.Children = Children;
        }

        private ClaimNavigatorItem()
        {
            // Private Constructor
        }

        public IClaimNavigatorItem BuildClaimTree(RoleClaims RoleClaims)
        {
            return new ClaimNavigatorItem()
                {
                    Key = Key,
                    Name = Name,
                    Description = Description,
                    Hidden = Hidden,
                    accessor = accessor,
                    Value = accessor == null ? (bool?)null : accessor(RoleClaims),
                    Children = Children == null ? null : Children.Cast<ClaimNavigatorItem>().Select(c => c.BuildClaimTree(RoleClaims)).ToList()
                };
        }

        public IClaimNavigatorItem BuildClaimTree(IEnumerable<RoleClaims> RoleClaims)
        {
            return new ClaimNavigatorItem()
            {
                Key = Key,
                Name = Name,
                Description = Description,
                Hidden = Hidden,
                accessor = accessor,
                Value = accessor == null ? (bool?)null : RoleClaims.Any(rc => accessor(rc)),
                Children = Children == null ? null : Children.Cast<ClaimNavigatorItem>().Select(c => c.BuildClaimTree(RoleClaims)).ToList()
            };
        }

        public override string ToString()
        {
            return $"{Name}: {Key}={Value}";
        }
    }
}
