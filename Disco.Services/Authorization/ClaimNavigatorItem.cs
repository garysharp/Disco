using Disco.Models.Authorization;
using Disco.Services.Authorization.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization
{
    public class ClaimNavigatorItem : IClaimNavigatorItem
    {
        private Func<Roles.RoleClaims, bool> accessor { get; set; }
        
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
            this.Name = details.Item1;
            this.Description = details.Item2;
            this.accessor = Claims.GetClaimAccessor(Key);
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
                    Key = this.Key,
                    Name = this.Name,
                    Description = this.Description,
                    Hidden = this.Hidden,
                    accessor = this.accessor,
                    Value = this.accessor == null ? (bool?)null : this.accessor(RoleClaims),
                    Children = this.Children == null ? null : this.Children.Cast<ClaimNavigatorItem>().Select(c => c.BuildClaimTree(RoleClaims)).ToList()
                };
        }

        public IClaimNavigatorItem BuildClaimTree(IEnumerable<RoleClaims> RoleClaims)
        {
            return new ClaimNavigatorItem()
            {
                Key = this.Key,
                Name = this.Name,
                Description = this.Description,
                Hidden = this.Hidden,
                accessor = this.accessor,
                Value = this.accessor == null ? (bool?)null : RoleClaims.Any(rc => this.accessor(rc)),
                Children = this.Children == null ? null : this.Children.Cast<ClaimNavigatorItem>().Select(c => c.BuildClaimTree(RoleClaims)).ToList()
            };
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}={2}", this.Name, this.Key, this.Value);
        }
    }
}
