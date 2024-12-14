namespace Disco.Models.ClientServices
{
    public class WhoAmI : ServiceBase<WhoAmIResponse>
    {

        public override string Feature
        {
            get { return "WhoAmI"; }
        }
    }
}
