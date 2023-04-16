using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Disco.Services.Expressions
{
    public static class ExpressionCache
    {
        private static ConcurrentDictionary<string, Expression> singleCache = new ConcurrentDictionary<string, Expression>();
        private static ConcurrentDictionary<string, Dictionary<string, Expression>> cache = new ConcurrentDictionary<string, Dictionary<string, Expression>>();
        private static ConcurrentDictionary<string, List<DocumentField>> fieldCache = new ConcurrentDictionary<string, List<DocumentField>>();

        private const string DocumentTemplateCacheTemplate = "DocumentTemplate_{0}";

        public static Expression GetOrCreateSingleExpressions(string key, Func<Expression> create)
        {
            if (singleCache.TryGetValue(key, out var result))
                return result;
            else
            {
                result = create();
                singleCache.TryAdd(key, result);
                return result;
            }
        }

        public static Dictionary<string, Expression> GetOrCreateExpressions(string module, Func<Tuple<Dictionary<string, Expression>, List<DocumentField>>> create)
        {
            if (cache.TryGetValue(module, out var result))
                return result;
            else
            {
                return Create(module, create).Item1;
            }
        }

        public static List<DocumentField> GetOrCreateFields(string module, Func<Tuple<Dictionary<string, Expression>, List<DocumentField>>> create)
        {
            if (fieldCache.TryGetValue(module, out var result))
                return result;
            else
            {
                return Create(module, create).Item2;
            }
        }

        public static void InvalidateCache(DocumentTemplate template)
        {
            InvalidateCache(string.Format(DocumentTemplateCacheTemplate, template.Id));
        }

        public static void InvalidateCache(string module)
        {
            cache.TryRemove(module, out _);
            fieldCache.TryRemove(module, out _);
        }

        public static void InvalidateSingleCache(string key)
        {
            singleCache.TryRemove(key, out _);
        }

        public static Dictionary<string, Expression> GetOrCreateExpressions(DocumentTemplate template, Func<Tuple<Dictionary<string, Expression>, List<DocumentField>>> create)
        {
            return GetOrCreateExpressions(string.Format(DocumentTemplateCacheTemplate, template.Id), create);
        }

        public static List<DocumentField> GetOrCreateFields(DocumentTemplate template, Func<Tuple<Dictionary<string, Expression>, List<DocumentField>>> create)
        {
            return GetOrCreateFields(string.Format(DocumentTemplateCacheTemplate, template.Id), create);
        }

        private static Tuple<Dictionary<string, Expression>, List<DocumentField>> Create(string module, Func<Tuple<Dictionary<string, Expression>, List<DocumentField>>> create)
        {
            var results = create();
            cache.TryAdd(module, results.Item1);
            fieldCache.TryAdd(module, results.Item2);
            return results;
        }
    }
}
