﻿using System;

namespace Disco.Web.Areas.Config.Models.SystemConfig
{
    public class ActivateModel
    {
        public Uri CallbackUrl { get; set; }
        public Guid DeploymentId { get; set; }
        public Guid CorrelationId { get; set; }
        public string UserId { get; set; }
    }
}
