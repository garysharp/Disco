using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spring.Expressions.Parser.antlr;

namespace Disco.BI.Expressions
{
    public class EvaluateExpressionParseException
    {
        public string Expression { get; set; }
        public int PositionRow { get; set; }
        public int PositionColumn { get; set; }
        public string Message { get; set; }

        internal static EvaluateExpressionParseException FromRecognitionException(RecognitionException e, string Expression)
        {
            return new EvaluateExpressionParseException()
            {
                Expression = Expression,
                Message = e.Message,
                PositionRow = e.getLine(),
                PositionColumn = e.getColumn()
            };
        }
    }
}
