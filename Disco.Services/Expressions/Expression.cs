using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Expressions.Extensions;
using Disco.Services.Plugins.Features.DetailsProvider;
using Spring.Core.TypeResolution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Disco.Services.Expressions
{
    public sealed class Expression : List<IExpressionPart>
    {
        public string Name { get; }
        public string Source { get; }
        public bool IsDynamic { get; private set; }
        public int Ordinal { get; }

        public bool IsRequired { get; }
        public bool IsReadOnly { get; }

        public RectangleF? Position { get; }

        private Expression(string name, string source, int ordinal, bool isRequired, bool isReadOnly, RectangleF? position)
        {
            Name = name;
            Source = source;
            Ordinal = ordinal;
            IsRequired = isRequired;
            IsReadOnly = isReadOnly;
            Position = position;
        }

        public static void InitializeExpressions()
        {
            TypeRegistry.RegisterType("DataExt", typeof(Extensions.DataExt));
            TypeRegistry.RegisterType("DeviceExt", typeof(Extensions.DeviceExt));
            TypeRegistry.RegisterType("EmailExt", typeof(Extensions.EmailExt));
            TypeRegistry.RegisterType("ImageExt", typeof(Extensions.ImageExt));
            TypeRegistry.RegisterType("UserExt", typeof(Extensions.UserExt));
        }

        public T EvaluateFirst<T>(object ExpressionContext, IDictionary Variables)
        {
            T result = default;
            if (Count > 0)
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
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Expression evaluation resulted in an error", ex);
                }
            }

            return result;
        }

        public Tuple<string, bool, object> Evaluate(object ExpressionContext, IDictionary Variables)
        {
            if (Count == 0)
                return Tuple.Create(string.Empty, false, (object)null);

            if (!IsDynamic)
            {
                if (Count != 1)
                    throw new InvalidOperationException("Non-dynamic expressions should only have one part");
                if (this[0] is TextExpressionPart textPart)
                    return Tuple.Create(textPart.RawSource, false, (object)null);
                else
                    throw new InvalidOperationException("Non-dynamic expressions should have a single TextExpressionPart component");
            }

            var resultValue = new StringBuilder();
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
                catch (Exception ex)
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
            return Tuple.Create(resultValue.ToString(), resultError, resultObject);
        }
        public static Expression TokenizeSingleDynamic(string Name, string ExpressionSource, int Ordinal)
        {
            var e = new Expression(Name, ExpressionSource, Ordinal, isRequired: false, isReadOnly: false, position: null);
            if (ExpressionSource != null && !string.IsNullOrWhiteSpace(ExpressionSource))
                e.Add(new EvaluateExpressionPart(ExpressionSource));
            e.IsDynamic = true;
            return e;
        }
        public static Expression Tokenize(string Name, string ExpressionSource, int Ordinal, bool IsRequired, bool IsReadOnly)
            => Tokenize(Name, ExpressionSource, Ordinal, IsRequired, IsReadOnly, null);

        public static Expression Tokenize(string name, string expressionSource, int ordinal, bool isRequired, bool isReadOnly, RectangleF? position)
        {
            var e = new Expression(name, expressionSource, ordinal, isRequired, isReadOnly, position);
            if (!expressionSource.Contains("{") || !expressionSource.Contains("}"))
            {
                e.Add(new TextExpressionPart(expressionSource));
            }
            else
            {
                var token = new StringBuilder();
                bool tokenEval = false;
                int tokenEvalDepth = 0;
                foreach (char c in expressionSource)
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
                                        token = new StringBuilder();
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
                                            token = new StringBuilder();
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

        public static IDictionary StandardVariables(DocumentTemplate AttachmentType, DiscoDataContext Database, User User, DateTime TimeStamp, DocumentState DocumentState, IAttachmentTarget target = null)
        {
            var detailsVariables = new Dictionary<string, object>();
            var detailsService = new DetailsProviderService(Database);
            if (target != null)
            {
                if (target is User targetUser)
                {
                    detailsVariables.Add("UserDetails", new LazyDictionary(() => detailsService.GetDetails(targetUser)));
                }
                else if (target is Job targetJob)
                {
                    detailsVariables.Add("UserDetails", targetJob.User == null ? (IDictionary<string, string>)new Dictionary<string, string>() : new LazyDictionary(() => detailsService.GetDetails(targetJob.User)));
                }
                else if (target is Device targetDevice)
                {
                    detailsVariables.Add("UserDetails", targetDevice.AssignedUser == null ? (IDictionary<string, string>)new Dictionary<string, string>() : new LazyDictionary(() => detailsService.GetDetails(targetDevice.AssignedUser)));
                }
            }

            return new Hashtable(detailsVariables)
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
                    typeof(DateTime).AssemblyQualifiedName
                },

                {
                    "#AttachmentType",
                    typeof(DocumentTemplate).AssemblyQualifiedName
                },

                {
                    "#State",
                    typeof(DocumentState).AssemblyQualifiedName
                },

                {
                    "#UserDetails",
                    typeof(Dictionary<string, string>).AssemblyQualifiedName
                },
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
                    "EmailExt",
                    typeof(Extensions.EmailExt).AssemblyQualifiedName
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
