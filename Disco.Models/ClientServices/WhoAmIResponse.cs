using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.ClientServices
{
    public class WhoAmIResponse
    {
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string Username { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", DisplayName, Username);
        }
    }
}
