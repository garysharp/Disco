using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Models.ClientServices;

namespace Disco.Client.Extensions
{
    public static class WhoAmIExtensions
    {

        public static void Process(this WhoAmIResponse whoAmIResponse)
        {
            Program.IsAuthenticated = true;
            whoAmIResponse.PresentResponse();
        }

        private static void PresentResponse(this WhoAmIResponse whoAmIResponse)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("Authenticated Connection:");
            message.Append("Username: ").AppendLine(whoAmIResponse.Username);
            message.Append("Name: ").Append(whoAmIResponse.DisplayName);
            message.Append(" (").Append(whoAmIResponse.Type).AppendLine(")");
            Presentation.UpdateStatus("Connection Established to Preparation Server", message.ToString(), false, 0, 1500);
        }
        public static void UnauthenticatedResponse()
        {
            Program.IsAuthenticated = false;
            Presentation.UpdateStatus("Connection Established to Preparation Server", "Unauthenticated connection to the server...", false, 0, 1500);
        }


    }
}
