using System;
using System.Collections;

namespace Disco.BI.Expressions
{
    public class TextExpressionPart : IExpressionPart
    {
        private string _Source;

        bool IExpressionPart.ErrorsAllowed
        {
            get
            {
                return false;
            }
            set
            {
                return;
            }
        }
        string IExpressionPart.Source
        {
            get
            {
                return this._Source;
            }
            set
            {
                return;
            }
        }
        string IExpressionPart.RawSource
        {
            get
            {
                return this._Source;
            }
            set
            {
                return;
            }
        }
        bool IExpressionPart.IsDynamic
        {
            get
            {
                return false;
            }
            set
            {
                return;
            }
        }
        public bool ParseError
        {
            get { return false; }
        }

        public string ParseErrorMessage
        {
            get { return null; }
        }

        public TextExpressionPart(string Source)
        {
            this._Source = Source;
        }
        object IExpressionPart.Evaluate(object ExpressionContext, System.Collections.IDictionary Variables)
        {
            return this._Source;
        }

    }
}
