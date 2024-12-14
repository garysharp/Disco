namespace Disco.Models.Services.Searching
{
    public interface ISearchResultItem
    {
        string Id { get; set; }
        string Type { get; }
        string Description { get; }
        string[] ScoreValues { get; }
    }
}
