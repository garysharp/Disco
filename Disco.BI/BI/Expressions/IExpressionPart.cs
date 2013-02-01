using System;
using System.Collections;

namespace Disco.BI.Expressions
{
	public interface IExpressionPart
	{
        string RawSource { get; set; }
		string Source { get; set; }
        bool ErrorsAllowed { get; set; }
        bool ParseError { get; }
        string ParseErrorMessage { get; }
		bool IsDynamic { get; set; }
		object Evaluate(object ExpressionContext, System.Collections.IDictionary Variables);
	}
}
