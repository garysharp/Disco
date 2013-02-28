using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Spring.Expressions.Parser.antlr;

namespace Disco.BI.Expressions
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
                        _EvaluateParseException = EvaluateExpressionParseException.FromRecognitionException(_ExpressionParseException, this.Source);
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
            this.RawSource = Source;

            if (Source.StartsWith("{") && Source.EndsWith("}"))
                Source = Source.Substring(1, Source.Length - 2);

            if (Source[0] == '~')
            {
                this.ErrorsAllowed = true;
                this.Source = Source.Substring(1);
            }
            else
            {
                this.ErrorsAllowed = false;
                this.Source = Source;
            }
            try
            {
                this._Expression = Spring.Expressions.Expression.Parse(this.Source);

            }
            catch (RecognitionException ex)
            {
                this._ExpressionParseException = ex;
            }
        }
        object IExpressionPart.Evaluate(object ExpressionContext, System.Collections.IDictionary Variables)
        {
            if (this._ExpressionParseException == null)
            {
                return this._Expression.GetValue(ExpressionContext, Variables);
            }
            throw this._ExpressionParseException;
        }

    }
}
