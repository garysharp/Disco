namespace Disco.ClientBootstrapper
{
    interface IStatus
    {
        void UpdateStatus(string Heading, string SubHeading, string Message, bool? ShowProgress = null, int? Progress = null);
    }
}
