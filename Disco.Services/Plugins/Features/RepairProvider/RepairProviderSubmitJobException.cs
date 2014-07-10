using System;

namespace Disco.Services.Plugins.Features.RepairProvider
{
    public class RepairProviderSubmitJobException : Exception
    {
        public RepairProviderSubmitJobException(string Message)
            : base(Message)
        {
        }
    }
}