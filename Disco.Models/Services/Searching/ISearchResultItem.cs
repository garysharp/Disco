using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Searching
{
    public interface ISearchResultItem
    {
        string Id { get; set; }
        string Type { get; }
        string Description { get; }
        string ScoreValue { get; }
    }
}
