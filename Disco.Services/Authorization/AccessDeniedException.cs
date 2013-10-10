using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization
{
    public class AccessDeniedException : Exception
    {
        private string _message { get; set; }

        public AccessDeniedException(string Message)
        {
            this._message = Message;
        }

        public override string Message
        {
            get
            {
                if (this._message == null)
                {
                    return "Your account does not have the required permission to access this Disco feature.";
                }
                else
                {
                    return this._message;
                }
            }
        }
    }
}
