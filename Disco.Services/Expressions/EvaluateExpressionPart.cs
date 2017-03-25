using Spring.Expressions.Parser.antlr;
using System.Collections;

namespace Disco.Services.Expressions
{
    public class EvaluateExpressionPart : IExpressionPart
    {
        private Spring.Expressions.IExpression _Expression;
        private RecognitionException _ExpressionParseException;
        private EvaluateExpressionParseException _EvaluateParseException;

        public string RawSource { get; set; }
        public string Source { get; set; }
        public bool ErrorsAllowed { get; set; }
        public bool IsDynamic { get { return true; } set { return; } }

        public EvaluateExpressionParseException ParseException
        {
            get
            {
                if (_ExpressionParseException == null)
                    return null;
                else
                    if (_EvaluateParseException == null)
                        _EvaluateParseException = EvaluateExpressionParseException.FromRecognitionException(_ExpressionParseException, Source);
                return _EvaluateParseException;
            }
        }

        public bool ParseError
        {
            get { return (_ExpressionParseException != null); }
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
                _Expression = Spring.Expressions.Expression.Parse(this.Source);

            }
            catch (RecognitionException ex)
            {
                _ExpressionParseException = ex;
            }
        }
        object IExpressionPart.Evaluate(object ExpressionContext, IDictionary Variables)
        {
            if (_ExpressionParseException == null)
            {
                return _Expression.GetValue(ExpressionContext, Variables);
            }
            throw _ExpressionParseException;
        }

    }
}
