using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.Shared
{
    public interface ConfigSharedTaskStatusModel : BaseUIModel
    {
        string SessionId { get; set; }
    }
}
