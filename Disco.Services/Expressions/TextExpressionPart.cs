namespace Disco.Services.Expressions
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
                return _Source;
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
                return _Source;
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
            _Source = Source;
        }
        object IExpressionPart.Evaluate(object ExpressionContext, System.Collections.IDictionary Variables)
        {
            return _Source;
        }

    }
}
