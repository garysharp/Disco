namespace Disco.Services.Expressions
{
    public class TextExpressionPart : IExpressionPart
    {
        private string _Source;

        public bool ErrorsAllowed
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
        public string Source
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
        public string RawSource
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
        public bool IsDynamic
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
