namespace Disco.Models.Services.Exporting
{
    public class ExportOptionField
    {
        public string GroupName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsChecked { get; set; }
        public string CustomKey { get; set; }
        public string CustomValue { get; set; }
    }
}
