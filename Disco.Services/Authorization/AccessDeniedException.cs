using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization
{
    public class AccessDeniedException : Exception
    {
        private string message { get; set; }
        private string resource { get; set; }

        public AccessDeniedException(string Message, string Resource)
        {
            this.message = Message;
            this.resource = Resource;
        }

        public override string Message
        {
            get
            {
                if (this.message == null)
                {
                    return "Your account does not have the required permission to access this Disco feature.";
                }
                else
                {
                    return this.message;
                }
            }
        }

        public string Resource
        {
            get
            {
                return this.resource;
            }
        }
    }
}
