using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Disco.BI.Expressions
{
    public static class ExpressionCache
    {
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, Expression>> _Cache = new ConcurrentDictionary<string, ConcurrentDictionary<string, Expression>>();

        public delegate Expression CreateValueDelegate();

        public static ConcurrentDictionary<string, Expression> GetModule(string Module, bool Create = false)
        {
            ConcurrentDictionary<string, Expression> moduleCache;
            if (_Cache.TryGetValue(Module, out moduleCache))
                return moduleCache;
            else
            {
                if (Create)
                {
                    moduleCache = new ConcurrentDictionary<string, Expression>();
                    _Cache.TryAdd(Module, moduleCache);
                    return moduleCache;
                }
                else
                    return null;
            }
        }
        private static Expression GetModuleValue(string Module, string Key, CreateValueDelegate CreateValue)
        {
            ConcurrentDictionary<string, Expression> moduleCache = GetModule(Module, (CreateValue != null));
            if (moduleCache != null)
            {
                Expression expression;
                if (moduleCache.TryGetValue(Key, out expression))
                {
                    return expression;
                }
                if (CreateValue != null)
                {
                    expression = CreateValue();
                    Expression oldExpression;
                    if (moduleCache.TryGetValue(Key, out oldExpression))
                        moduleCache.TryUpdate(Key, expression, oldExpression);
                    else
                        moduleCache.TryAdd(Key, expression);
                    return expression;
                }
            }
            return null;
        }

        public static Expression GetValue(string Module, string Key, CreateValueDelegate CreateValue)
        {
            return GetModuleValue(Module, Key, CreateValue);
        }

        public static Expression GetValue(string Module, string Key)
        {
            return GetModuleValue(Module, Key, null);
        }

        public static bool InvalidModule(string Module)
        {
            ConcurrentDictionary<string, Expression> moduleCache;
            return _Cache.TryRemove(Module, out moduleCache);
        }

        public static bool InvalidateKey(string Module, string Key)
        {
            Expression expression;
            ConcurrentDictionary<string, Expression> moduleCache = GetModule(Module, false);
            if (moduleCache != null)
            {
                bool removeResult = moduleCache.TryRemove(Key, out expression);
                if (moduleCache.Count == 0)
                    InvalidModule(Module);
                return removeResult;
            }
            else
                return false;
        }

        public static bool SetValue(string Module, string Key, Expression Expression)
        {
            ConcurrentDictionary<string, Expression> moduleCache = GetModule(Module, true);

            if (moduleCache.ContainsKey(Key))
            {
                Expression oldExpression;
                if (moduleCache.TryGetValue(Key, out oldExpression))
                {
                    return moduleCache.TryUpdate(Key, Expression, oldExpression);
                }
            }
            return moduleCache.TryAdd(Key, Expression);
        }

    }
}
