namespace Disco.Models.ClientServices
{
    public class WhoAmIResponse
    {
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string Username { get; set; }

        public override string ToString()
        {
            return $"{DisplayName} ({Username})";
        }
    }
}
