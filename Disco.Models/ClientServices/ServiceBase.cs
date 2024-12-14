namespace Disco.Models.ClientServices
{
    public abstract class ServiceBase<ResponseType>
    {
        internal ServiceBase()
        {
        }

        public abstract string Feature { get; }
    }
}
