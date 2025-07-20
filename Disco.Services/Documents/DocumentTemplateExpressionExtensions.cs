using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services.Documents;
using Disco.Services.Expressions;
using System;
using System.Collections.Generic;

namespace Disco.Services
{
    public static class DocumentTemplateExpressionExtensions
    {
        public static Expression FilterExpressionFromCache(this DocumentTemplate dt)
        {
            return ExpressionCache.GetOrCreateSingleExpressions($"DocumentTemplate_FilterExpression_{dt.Id}", () => Expression.TokenizeSingleDynamic(null, dt.FilterExpression, 0));
        }

        public static void FilterExpressionInvalidateCache(this DocumentTemplate dt)
        {
            ExpressionCache.InvalidateSingleCache($"DocumentTemplate_FilterExpression_{dt.Id}");
        }

        public static bool FilterExpressionMatches(this DocumentTemplate dt, IAttachmentTarget Data, DiscoDataContext Database, User User, DateTime TimeStamp, DocumentState State)
        {
            if (!string.IsNullOrEmpty(dt.FilterExpression))
            {
                var compiledExpression = dt.FilterExpressionFromCache();
                var evaluatorVariables = Expression.StandardVariables(dt, Database, User, TimeStamp, State, Data);
                try
                {
                    var er = compiledExpression.EvaluateFirst<object>(Data, evaluatorVariables);
                    if (er is bool erBool)
                    {
                        return erBool;
                    }
                    if (bool.TryParse(er.ToString(), out erBool))
                    {
                        return erBool;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public static Expression OnImportAttachmentExpressionFromCache(this DocumentTemplate dt)
        {
            return ExpressionCache.GetOrCreateSingleExpressions($"DocumentTemplate_OnImportExpression_{dt.Id}", () => Expression.TokenizeSingleDynamic(null, dt.OnImportAttachmentExpression, 0));
        }

        public static void OnImportAttachmentExpressionInvalidateCache(this DocumentTemplate dt)
        {
            ExpressionCache.InvalidateSingleCache($"DocumentTemplate_OnImportExpression_{dt.Id}");
        }

        public static string EvaluateOnAttachmentImportExpression(this DocumentTemplate dt, IAttachment Data, IAttachmentTarget AttachmentTarget, DiscoDataContext Database, User User, DateTime TimeStamp, List<DocumentUniqueIdentifier> PageIdentifiers)
        {
            if (!string.IsNullOrEmpty(dt.OnImportAttachmentExpression))
            {
                var compiledExpression = dt.OnImportAttachmentExpressionFromCache();
                var evaluatorVariables = Expression.StandardVariables(dt, Database, User, TimeStamp, null, AttachmentTarget);
                evaluatorVariables.Add("PageIdentifiers", PageIdentifiers);
                var result = compiledExpression.EvaluateFirst<object>(Data, evaluatorVariables);
                if (result == null)
                    return null;
                else
                    return result.ToString();
            }
            return null;
        }

        public static Expression OnGenerateExpressionFromCache(this DocumentTemplate dt)
        {
            return ExpressionCache.GetOrCreateSingleExpressions($"DocumentTemplate_OnGenerateExpression_{dt.Id}", () => Expression.TokenizeSingleDynamic(null, dt.OnGenerateExpression, 0));
        }

        public static void OnGenerateExpressionInvalidateCache(this DocumentTemplate dt)
        {
            ExpressionCache.InvalidateSingleCache($"DocumentTemplate_OnGenerateExpression_{dt.Id}");
        }

        public static string EvaluateOnGenerateExpression(this DocumentTemplate dt, IAttachmentTarget Data, DiscoDataContext Database, User User, DateTime TimeStamp, DocumentState State)
        {
            if (!string.IsNullOrEmpty(dt.OnGenerateExpression))
            {
                var compiledExpression = dt.OnGenerateExpressionFromCache();
                var evaluatorVariables = Expression.StandardVariables(dt, Database, User, TimeStamp, State, Data);

                var result = compiledExpression.EvaluateFirst<object>(Data, evaluatorVariables);
                return result.ToString();
            }
            return null;
        }
    }
}
