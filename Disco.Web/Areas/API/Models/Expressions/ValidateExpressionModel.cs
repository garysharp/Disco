using Disco.Services.Expressions;
using System.Web;

namespace Disco.Web.Areas.API.Models.Expressions
{
    public class ValidateExpressionModel
    {
        public string Expression { get; set; }
        public bool ExpressionValid { get; set; }
        public int PositionRow { get; set; }
        public int PositionColumn { get; set; }
        public string Message { get; set; }
        public string MessageHtmlEncoded { get; set; }

        public static ValidateExpressionModel FromEvaluateExpressionPart(EvaluateExpressionPart part)
        {
            var parseException = part.ParseException;
            if (parseException != null)
            {
                return new ValidateExpressionModel()
                {
                    Expression = part.Source,
                    ExpressionValid = false,
                    Message = parseException.Message,
                    MessageHtmlEncoded = HttpUtility.HtmlEncode(parseException.Message),
                    PositionRow = parseException.PositionRow,
                    PositionColumn = parseException.PositionColumn
                };
            }
            else
                return new ValidateExpressionModel() { Expression = part.Source, ExpressionValid = true };
        }
    }
}