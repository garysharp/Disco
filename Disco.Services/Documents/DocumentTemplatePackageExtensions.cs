using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Services.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services
{
    public static class DocumentTemplatePackageExtensions
    {
        public static List<JobSubType> GetJobSubTypes(this DocumentTemplatePackage package, IEnumerable<JobSubType> JobSubTypes)
        {
            var result = new List<JobSubType>();

            if (package.JobSubTypes != null && package.JobSubTypes.Count > 0)
            {
                foreach (var jobSubTypeRefId in package.JobSubTypes)
                {
                    var jobTypeId = jobSubTypeRefId.Substring(0, jobSubTypeRefId.IndexOf('_'));
                    var jobSubTypeId = jobSubTypeRefId.Substring(jobTypeId.Length + 1);
                    result.Add(JobSubTypes.First(jst => jst.JobTypeId == jobTypeId && jst.Id == jobSubTypeId));
                }
            }

            return result;
        }

        public static List<DocumentTemplate> GetDocumentTemplates(this DocumentTemplatePackage package, DiscoDataContext Database)
        {
            var result = new List<DocumentTemplate>();

            if (package.DocumentTemplateIds != null && package.DocumentTemplateIds.Count > 0)
            {
                var dbScope = package.Scope.ToString();
                var dbTemplates = Database.DocumentTemplates
                    .Where(dt => package.DocumentTemplateIds.Contains(dt.Id) && dt.Scope == dbScope)
                    .ToList();

                foreach (var id in package.DocumentTemplateIds)
                {
                    var template = dbTemplates.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                    if (template != null)
                    {
                        result.Add(template);
                    }
                }
            }

            return result;
        }

        public static List<DocumentTemplate> GetDocumentTemplates(this DocumentTemplatePackage package, IEnumerable<DocumentTemplate> DocumentTemplates)
        {
            var result = new List<DocumentTemplate>();

            if (package.DocumentTemplateIds != null && package.DocumentTemplateIds.Count > 0)
            {
                var dbScope = package.Scope.ToString();
                foreach (var id in package.DocumentTemplateIds)
                {
                    var template = DocumentTemplates.FirstOrDefault(t => t.Id == id && t.Scope == dbScope);
                    if (template != null)
                    {
                        result.Add(template);
                    }
                }
            }

            return result;
        }

        public static Expression FilterExpressionFromCache(this DocumentTemplatePackage package)
        {
            return ExpressionCache.GetValue("DocumentTemplatePackage_FilterExpression", package.Id, () => { return Expression.TokenizeSingleDynamic(null, package.FilterExpression, 0); });
        }

        public static void FilterExpressionInvalidateCache(this DocumentTemplatePackage package)
        {
            ExpressionCache.InvalidateKey("DocumentTemplatePackage_FilterExpression", package.Id);
        }

        public static bool FilterExpressionMatches(this DocumentTemplatePackage package, object Data, DiscoDataContext Database, User User, DateTime TimeStamp, DocumentState State)
        {
            if (!string.IsNullOrEmpty(package.FilterExpression))
            {
                var compiledExpression = package.FilterExpressionFromCache();
                var evaluatorVariables = Expression.StandardVariables(null, Database, User, TimeStamp, State);
                evaluatorVariables.Add("Package", package);
                try
                {
                    object er = compiledExpression.EvaluateFirst<object>(Data, evaluatorVariables);
                    if (er is bool)
                    {
                        return (bool)er;
                    }
                    bool erBool;
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

        public static Expression OnGenerateExpressionFromCache(this DocumentTemplatePackage package)
        {
            return ExpressionCache.GetValue("DocumentTemplatePackage_OnGenerateExpression", package.Id, () => { return Expression.TokenizeSingleDynamic(null, package.OnGenerateExpression, 0); });
        }

        public static void OnGenerateExpressionInvalidateCache(this DocumentTemplatePackage package)
        {
            ExpressionCache.InvalidateKey("DocumentTemplatePackage_OnGenerateExpression", package.Id);
        }

        public static string EvaluateOnGenerateExpression(this DocumentTemplatePackage package, object Data, DiscoDataContext Database, User User, DateTime TimeStamp, DocumentState State)
        {
            if (!string.IsNullOrEmpty(package.OnGenerateExpression))
            {
                Expression compiledExpression = package.OnGenerateExpressionFromCache();
                System.Collections.IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, User, TimeStamp, State);
                evaluatorVariables.Add("Package", package);
                try
                {
                    object result = compiledExpression.EvaluateFirst<object>(Data, evaluatorVariables);
                    return result.ToString();
                }
                catch
                {
                    throw;
                }
            }
            return null;
        }

        public static IAttachmentTarget ResolveScopeTarget(this DocumentTemplatePackage templatePackage, DiscoDataContext database, string targetId)
        {
            if (templatePackage == null)
                throw new ArgumentNullException(nameof(templatePackage));

            return templatePackage.Scope.ResolveScopeTarget(database, targetId);
        }

    }
}
