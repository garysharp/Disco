using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.Expressions
{
    public class EditorModel
    {
        public string Expression { get; set; }
        public Disco.Web.Areas.API.Models.Expressions.ValidateExpressionModel ExpressionException { get; set; }
        public string TestScope { get; set; }
        public string TestScopeDataType { get; set; }
        public string TestScopeDataId { get; set; }
    }
}