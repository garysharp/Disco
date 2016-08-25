using System.Collections;

namespace Disco.Services.Expressions
{
	public interface IExpressionPart
	{
        string RawSource { get; set; }
		string Source { get; set; }
        bool ErrorsAllowed { get; set; }
        bool ParseError { get; }
        string ParseErrorMessage { get; }
		bool IsDynamic { get; set; }
		object Evaluate(object ExpressionContext, IDictionary Variables);
	}
}
