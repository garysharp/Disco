using System.Collections.Generic;

namespace Disco.Web.Areas.API.Models.DeviceModel
{
    public class TestComputerNameTemplateModel
    {
        public int DeviceProfileId { get; set; }
        public string ComputerNameTemplate { get; set; }
        public TestComputerNameTemplateResultModel UserSpecifiedResult { get; set; }
        public List<TestComputerNameTemplateResultModel> RandomDeviceResults { get; set; }

        public class TestComputerNameTemplateResultModel
        {
            public string DeviceSerialNumber { get; set; }
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string DeviceComputerName { get; set; }
            public string RenderedComputerName { get; set; }
            public string Url { get; set; }
        }

    }
}
