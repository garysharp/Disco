using System;

namespace Disco.ClientBootstrapper
{
    interface IStatus
    {
        void UpdateStatus(string Heading, string SubHeading, string Message, Nullable<bool> ShowProgress = null, Nullable<int> Progress = null);
    }
}
