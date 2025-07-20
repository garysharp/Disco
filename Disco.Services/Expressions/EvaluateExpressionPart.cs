using Spring.Expressions.Parser.antlr;
using System.Collections;

namespace Disco.Services.Expressions
{
    public class EvaluateExpressionPart : IExpressionPart
    {
        private Spring.Expressions.IExpression expression;
        private RecognitionException expressionParseException;
        private EvaluateExpressionParseException evaluateParseException;

        public string RawSource { get; set; }
        public string Source { get; set; }
        public bool ErrorsAllowed { get; set; }
        public bool IsDynamic { get { return true; } set { return; } }

        public EvaluateExpressionParseException ParseException
        {
            get
            {
                if (expressionParseException == null)
                    return null;
                else if (evaluateParseException == null)
                    evaluateParseException = EvaluateExpressionParseException.FromRecognitionException(expressionParseException, Source);
                return evaluateParseException;
            }
        }

        public bool ParseError
        {
            get { return (expressionParseException != null); }
        }
        public string ParseErrorMessage
        {
            get
            {
                if (ParseError)
                    return ParseException.Message;
                else
                    return null;
            }
        }

        public EvaluateExpressionPart(string Source)
        {
            RawSource = Source;

            if (Source.StartsWith("{") && Source.EndsWith("}"))
                Source = Source.Substring(1, Source.Length - 2);

            if (Source[0] == '~')
            {
                ErrorsAllowed = true;
                this.Source = Source.Substring(1);
            }
            else
            {
                ErrorsAllowed = false;
                this.Source = Source;
            }
            try
            {
                expression = Spring.Expressions.Expression.Parse(this.Source);

            }
            catch (RecognitionException ex)
            {
                expressionParseException = ex;
            }
        }
        object IExpressionPart.Evaluate(object ExpressionContext, IDictionary Variables)
        {
            if (expressionParseException == null)
            {
                return expression.GetValue(ExpressionContext, Variables);
            }
            throw expressionParseException;
        }

    }
}
