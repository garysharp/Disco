using Disco.Models.Services.Devices.Importing;
using Disco.Services.Tasks;

namespace Disco.Services.Devices.Importing
{
    public interface IDeviceImportContextActions : IDeviceImportContext
    {
        bool AllowBacktracking { get; }

        void Parse(IScheduledTaskStatus status);
        void Apply(IScheduledTaskStatus status);
    }
}
