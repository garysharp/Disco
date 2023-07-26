using System;

namespace Disco.Services.Authorization
{
    public class AccessDeniedException : Exception
    {
        private string message { get; set; }
        private string resource { get; set; }

        public AccessDeniedException(string Message, string Resource)
        {
            message = Message;
            resource = Resource;
        }

        public override string Message
        {
            get
            {
                if (message == null)
                {
                    return "Your account does not have the required permission to access this Disco ICT feature.";
                }
                else
                {
                    return message;
                }
            }
        }

        public string Resource
        {
            get
            {
                return resource;
            }
        }
    }
}
