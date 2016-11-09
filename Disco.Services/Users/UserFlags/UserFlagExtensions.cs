using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Expressions;
using Disco.Services.Logging;
using Disco.Services.Users;
using System;
using System.Collections;
using System.Linq;

namespace Disco.Services
{
    public static class UserFlagExtensions
    {

        #region Edit Comments
        public static bool CanEditComments(this UserFlagAssignment fa)
        {
            return UserService.CurrentAuthorization.Has(Claims.User.Actions.EditFlags);
        }
        public static void OnEditComments(this UserFlagAssignment fa, string Comments)
        {
            if (!fa.CanEditComments())
                throw new InvalidOperationException("Editing comments for user flags is denied");

            fa.Comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim();
        }
        #endregion

        #region Remove
        public static bool CanRemove(this UserFlagAssignment fa)
        {
            if (fa.RemovedDate.HasValue)
                return false;

            return UserService.CurrentAuthorization.Has(Claims.User.Actions.RemoveFlags);
        }
        public static void OnRemove(this UserFlagAssignment fa, DiscoDataContext Database, User RemovingUser)
        {
            if (!fa.CanRemove())
                throw new InvalidOperationException("Removing user flags is denied");

            fa.OnRemoveUnsafe(Database, RemovingUser);
        }

        public static void OnRemoveUnsafe(this UserFlagAssignment fa, DiscoDataContext Database, User RemovingUser)
        {
            fa.RemovedDate = DateTime.Now;
            fa.RemovedUserId = RemovingUser.UserId;

            if (!string.IsNullOrWhiteSpace(fa.UserFlag.OnUnassignmentExpression))
            {
                try
                {
                    Database.SaveChanges();
                    var expressionResult = fa.EvaluateOnUnassignmentExpression(Database, RemovingUser, fa.AddedDate);
                    if (!string.IsNullOrWhiteSpace(expressionResult))
                    {
                        fa.OnUnassignmentExpressionResult = expressionResult;
                        Database.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    SystemLog.LogException("User Flag Expression - OnUnassignmentExpression", ex);
                }
            }
        }
        #endregion

        #region Add
        public static bool CanAddUserFlags(this User u)
        {
            return UserService.CurrentAuthorization.Has(Claims.User.Actions.AddFlags);
        }
        public static bool CanAddUserFlag(this User u, UserFlag flag)
        {
            // Shortcut
            if (!u.CanAddUserFlags())
                return false;

            // Already has User Flag?
            if (u.UserFlagAssignments.Any(fa => !fa.RemovedDate.HasValue && fa.UserFlagId == flag.Id))
                return false;

            return true;
        }
        public static UserFlagAssignment OnAddUserFlag(this User u, DiscoDataContext Database, UserFlag flag, User AddingUser, string Comments)
        {
            if (!u.CanAddUserFlag(flag))
                throw new InvalidOperationException("Adding user flag is denied");

            return u.OnAddUserFlagUnsafe(Database, flag, AddingUser, Comments);
        }

        public static UserFlagAssignment OnAddUserFlagUnsafe(this User u, DiscoDataContext Database, UserFlag flag, User AddingUser, string Comments)
        {
            var fa = new UserFlagAssignment()
            {
                UserFlag = flag,
                User = u,
                AddedDate = DateTime.Now,
                AddedUser = AddingUser,
                Comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim()
            };

            Database.UserFlagAssignments.Add(fa);

            if (!string.IsNullOrWhiteSpace(flag.OnAssignmentExpression))
            {
                try
                {
                    Database.SaveChanges();
                    var expressionResult = fa.EvaluateOnAssignmentExpression(Database, AddingUser, fa.AddedDate);
                    if (!string.IsNullOrWhiteSpace(expressionResult))
                    {
                        fa.OnAssignmentExpressionResult = expressionResult;
                        Database.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    SystemLog.LogException("User Flag Expression - OnAssignmentExpression", ex);
                }
            }

            return fa;
        }
        #endregion

        #region Expressions

        public static Expression OnAssignmentExpressionFromCache(this UserFlag uf)
        {
            return ExpressionCache.GetValue("UserFlag_OnAssignmentExpression", uf.Id.ToString(), () => { return Expression.TokenizeSingleDynamic(null, uf.OnAssignmentExpression, 0); });
        }

        public static void OnAssignmentExpressionInvalidateCache(this UserFlag uf)
        {
            ExpressionCache.InvalidateKey("UserFlag_OnAssignmentExpression", uf.Id.ToString());
        }

        public static string EvaluateOnAssignmentExpression(this UserFlagAssignment ufa, DiscoDataContext Database, User AddingUser, DateTime TimeStamp)
        {
            if (!string.IsNullOrEmpty(ufa.UserFlag.OnAssignmentExpression))
            {
                Expression compiledExpression = ufa.UserFlag.OnAssignmentExpressionFromCache();
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, AddingUser, TimeStamp, null);
                object result = compiledExpression.EvaluateFirst<object>(ufa, evaluatorVariables);
                if (result == null)
                    return null;
                else
                    return result.ToString();
            }
            return null;
        }

        public static Expression OnUnassignmentExpressionFromCache(this UserFlag uf)
        {
            return ExpressionCache.GetValue("UserFlag_OnUnassignmentExpression", uf.Id.ToString(), () => { return Expression.TokenizeSingleDynamic(null, uf.OnUnassignmentExpression, 0); });
        }

        public static void OnUnassignmentExpressionInvalidateCache(this UserFlag uf)
        {
            ExpressionCache.InvalidateKey("UserFlag_OnUnassignmentExpression", uf.Id.ToString());
        }

        public static string EvaluateOnUnassignmentExpression(this UserFlagAssignment ufa, DiscoDataContext Database, User RemovingUser, DateTime TimeStamp)
        {
            if (!string.IsNullOrEmpty(ufa.UserFlag.OnUnassignmentExpression))
            {
                Expression compiledExpression = ufa.UserFlag.OnUnassignmentExpressionFromCache();
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, RemovingUser, TimeStamp, null);
                object result = compiledExpression.EvaluateFirst<object>(ufa, evaluatorVariables);
                if (result == null)
                    return null;
                else
                    return result.ToString();
            }
            return null;
        }

        #endregion
    }
}
