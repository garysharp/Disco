using Disco.Data.Repository;
using Disco.Models.BI.DocumentTemplates;
using Disco.Models.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Disco.Models.BI.Expressions;

namespace Disco.BI.Expressions
{
    public sealed class Expression : System.Collections.Generic.List<IExpressionPart>
    {
        public string Name { get; private set; }
        public string Source { get; private set; }
        public bool IsDynamic { get; private set; }
        public int Ordinal { get; private set; }

        private Expression(string Name, string Source, int Ordinal)
        {
            this.Name = Name;
            this.Source = Source;
            this.Ordinal = Ordinal;
        }

        public static void InitializeExpressions()
        {
            Spring.Core.TypeResolution.TypeRegistry.RegisterType("DataExt", typeof(Extensions.DataExt));
            Spring.Core.TypeResolution.TypeRegistry.RegisterType("UserExt", typeof(Extensions.UserExt));
            Spring.Core.TypeResolution.TypeRegistry.RegisterType("DeviceExt", typeof(Extensions.DeviceExt));
            Spring.Core.TypeResolution.TypeRegistry.RegisterType("ImageExt", typeof(Extensions.ImageExt));
        }

        public T EvaluateFirst<T>(object ExpressionContext, System.Collections.IDictionary Variables)
        {
            T result = default(T);
            if (this.Count > 0)
            {
                try
                {
                    object expressionResult = this[0].Evaluate(ExpressionContext, Variables);
                    if (expressionResult != null)
                    {
                        if (expressionResult is T)
                        {
                            result = (T)expressionResult;
                        }
                        else
                        {
                            throw new InvalidOperationException("Expression returned an invalid type");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    throw new InvalidOperationException("Expression evaluation resulted in an error", ex);
                }
            }

            return result;
        }

        public Tuple<string, bool, object> Evaluate(object ExpressionContext, System.Collections.IDictionary Variables)
        {
            System.Text.StringBuilder resultValue = new System.Text.StringBuilder();
            object resultObject = null;
            bool resultError = false;
            foreach (var expressionPart in this)
            {
                try
                {
                    object partValue = expressionPart.Evaluate(ExpressionContext, Variables);
                    if (partValue != null)
                    {
                        // Check for Result Objects
                        if (partValue is IImageExpressionResult)
                            resultObject = partValue;
                        else
                            resultValue.Append(partValue.ToString());
                    }
                }
                catch (System.Exception ex)
                {
                    if (!expressionPart.ErrorsAllowed)
                    {
                        resultValue.Append("## ERROR # ");
                        resultValue.Append(ex.Message);
                        resultValue.Append(" ##");
                        resultError = true;
                    }
                }
            }
            return new Tuple<string, bool, object>(resultValue.ToString(), resultError, resultObject);
        }
        public static Expression TokenizeSingleDynamic(string Name, string ExpressionSource, int Ordinal)
        {
            Expression e = new Expression(Name, ExpressionSource, Ordinal);
            if (ExpressionSource != null && !string.IsNullOrWhiteSpace(ExpressionSource))
                e.Add(new EvaluateExpressionPart(ExpressionSource));
            e.IsDynamic = true;
            return e;
        }
        public static Expression Tokenize(string Name, string ExpressionSource, int Ordinal)
        {
            Expression e = new Expression(Name, ExpressionSource, Ordinal);
            if (!ExpressionSource.Contains("{") || !ExpressionSource.Contains("}"))
            {
                e.Add(new TextExpressionPart(ExpressionSource));
            }
            else
            {
                System.Text.StringBuilder token = new System.Text.StringBuilder();
                bool tokenEval = false;
                int tokenEvalDepth = 0;
                foreach (char c in ExpressionSource)
                {
                    switch (c)
                    {
                        case '{':
                            {
                                if (!tokenEval)
                                {
                                    if (token.Length > 0)
                                    {
                                        e.Add(new TextExpressionPart(token.ToString()));
                                        token = new System.Text.StringBuilder();
                                    }
                                    tokenEval = true;
                                    tokenEvalDepth = 0;
                                }
                                tokenEvalDepth++;
                                token.Append(c);
                                break;
                            }
                        case '}':
                            {
                                token.Append(c);
                                if (tokenEval)
                                {
                                    tokenEvalDepth--;
                                    if (tokenEvalDepth <= 0)
                                    {
                                        if (token.Length != 2 && (token.Length != 3 || token[1] != '@'))
                                        {
                                            e.Add(new EvaluateExpressionPart(token.ToString()));
                                            e.IsDynamic = true;
                                            token = new System.Text.StringBuilder();
                                        }
                                        tokenEval = false;
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                token.Append(c);
                                break;
                            }
                    }
                }
                if (token.Length > 0)
                {
                    e.Add(new TextExpressionPart(token.ToString()));
                }
            }
            return e;
        }

        public static IDictionary StandardVariables(DocumentTemplate AttachmentType, DiscoDataContext Database, User User, System.DateTime TimeStamp, DocumentState DocumentState)
        {
            return new Hashtable
			{

				{
					"DataContext", 
					Database
				}, 

				{
					"User", 
					User
				}, 

				{
					"TimeStamp", 
					TimeStamp
				}, 

				{
					"AttachmentType", 
					AttachmentType
				}, 

				{
					"State", 
					DocumentState
				}
			};
        }
        public static Dictionary<string, string> StandardVariableTypes()
        {
            return new Dictionary<string, string>
			{

				{
					"#DataContext", 
					typeof(DiscoDataContext).AssemblyQualifiedName
				}, 

				{
					"#User", 
					typeof(User).AssemblyQualifiedName
				}, 

				{
					"#TimeStamp", 
					typeof(System.DateTime).AssemblyQualifiedName
				}, 

				{
					"#AttachmentType", 
					typeof(DocumentTemplate).AssemblyQualifiedName
				}, 

				{
					"#State", 
					typeof(DocumentState).AssemblyQualifiedName
				}
			};
        }
        public static Dictionary<string, string> ExtensionLibraryTypes()
        {
            return new Dictionary<string, string>
			{
				{
					"DataExt", 
					typeof(Extensions.DataExt).AssemblyQualifiedName
				}, 

				{
					"DeviceExt", 
					typeof(Extensions.DeviceExt).AssemblyQualifiedName
				}, 
                
                {
					"ImageExt", 
					typeof(Extensions.ImageExt).AssemblyQualifiedName
				}, 
                
                {
					"UserExt", 
					typeof(Extensions.UserExt).AssemblyQualifiedName
				}
			};
        }

    }
}
