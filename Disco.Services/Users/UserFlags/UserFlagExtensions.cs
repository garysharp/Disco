using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Expressions;
using Disco.Services.Logging;
using Disco.Services.Users;
using Disco.Services.Users.UserFlags;
using System;
using System.Collections;
using System.Linq;

namespace Disco.Services
{
    public static class UserFlagExtensions
    {

        #region Edit
        public static bool CanEdit(this UserFlagAssignment fa)
        {
            var (_, permission) = UserFlagService.GetUserFlag(fa.UserFlagId);

            return permission.CanEdit();
        }
        public static void OnEdit(this UserFlagAssignment fa, string comments, DateTime? removeDate)
        {
            if (!fa.CanEdit())
                throw new InvalidOperationException("Editing comments for user flags is denied");

            fa.Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();

            if (fa.CanRemove() && removeDate != fa.RemoveDate &&
                (!removeDate.HasValue || removeDate.Value >= DateTime.Today))
            {
                fa.RemoveDate = removeDate?.Date;
                fa.RemoveUserId = UserService.CurrentUser.UserId;
            }
        }
        #endregion

        #region Remove
        public static bool CanRemove(this UserFlagAssignment fa)
        {
            if (fa.RemovedDate.HasValue)
                return false;

            var (_, permission) = UserFlagService.GetUserFlag(fa.UserFlagId);

            return permission.CanRemove();
        }
        public static void OnRemove(this UserFlagAssignment fa, DiscoDataContext database)
        {
            if (!fa.CanRemove())
                throw new InvalidOperationException("Removing user flags is denied");

            fa.OnRemoveUnsafe(database, UserService.CurrentUser);
        }

        public static void OnRemoveUnsafe(this UserFlagAssignment fa, DiscoDataContext database, User removingUser)
        {
            OnRemoveUnsafe(fa, database, removingUser, false);
        }

        internal static void OnRemoveUnsafe(this UserFlagAssignment fa, DiscoDataContext database, User removingUser, bool isScheduled)
        {
            fa = database.UserFlagAssignments.First(a => a.Id == fa.Id);
            removingUser = database.Users.First(u => u.UserId == removingUser.UserId);

            fa.RemovedDate = DateTime.Now;
            fa.RemovedUserId = removingUser.UserId;

            if (!isScheduled)
            {
                fa.RemoveDate = null;
                fa.RemoveUser = null;
                fa.RemoveUserId = null;
            }

            if (!string.IsNullOrWhiteSpace(fa.UserFlag.OnUnassignmentExpression))
            {
                try
                {
                    database.SaveChanges();
                    var expressionResult = fa.EvaluateOnUnassignmentExpression(database, removingUser, fa.AddedDate);
                    if (!string.IsNullOrWhiteSpace(expressionResult))
                    {
                        fa.OnUnassignmentExpressionResult = expressionResult;
                        database.SaveChanges();
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
        public static bool CanAddUserFlag(this User u, UserFlag flag)
        {
            // Already has User Flag?
            if (u.UserFlagAssignments.Any(fa => !fa.RemovedDate.HasValue && fa.UserFlagId == flag.Id))
                return false;

            var (_, permission) = UserFlagService.GetUserFlag(flag.Id);

            return permission.CanAssign();
        }
        public static UserFlagAssignment OnAddUserFlag(this User u, DiscoDataContext database, UserFlag flag, string comments)
        {
            var removeDate = (DateTime?)null;
            if (flag.DefaultRemoveDays.HasValue)
                removeDate = DateTime.Today.AddDays(flag.DefaultRemoveDays.Value);

            return OnAddUserFlag(u, database, flag, comments, removeDate);
        }
        public static UserFlagAssignment OnAddUserFlag(this User u, DiscoDataContext database, UserFlag flag, string comments, DateTime? removeDate)
        {
            if (!u.CanAddUserFlag(flag))
                throw new InvalidOperationException("Adding user flag is denied");

            if (u.CanRemoveUserFlag(flag))
                return u.OnAddUserFlagUnsafe(database, flag, UserService.CurrentUser, comments, removeDate);
            else
                return u.OnAddUserFlagUnsafe(database, flag, UserService.CurrentUser, comments);
        }
        public static bool CanRemoveUserFlag(this User u, UserFlag flag)
        {
            var (_, permission) = UserFlagService.GetUserFlag(flag.Id);

            return permission.CanRemove();
        }

        public static UserFlagAssignment OnAddUserFlagUnsafe(this User u, DiscoDataContext database, UserFlag flag, User addingUser, string comments)
        {
            var removeDate = (DateTime?)null;
            if (flag.DefaultRemoveDays.HasValue)
                removeDate = DateTime.Today.AddDays(flag.DefaultRemoveDays.Value);

            return OnAddUserFlagUnsafe(u, database, flag, addingUser, comments, removeDate);
        }

        public static UserFlagAssignment OnAddUserFlagUnsafe(this User u, DiscoDataContext database, UserFlag flag, User addingUser, string comments, DateTime? removeDate)
        {
            flag = database.UserFlags.First(f => f.Id == flag.Id);
            u = database.Users.First(user => user.UserId == u.UserId);
            addingUser = database.Users.First(user => user.UserId == addingUser.UserId);

            var fa = new UserFlagAssignment()
            {
                UserFlag = flag,
                User = u,
                AddedDate = DateTime.Now,
                AddedUser = addingUser,
                AddedUserId = addingUser.UserId,
                Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim()
            };

            if (removeDate.HasValue)
            {
                fa.RemoveDate = removeDate.Value.Date;
                fa.RemoveUser = addingUser;
                fa.RemoveUserId = addingUser.UserId;
            }

            database.UserFlagAssignments.Add(fa);

            if (!string.IsNullOrWhiteSpace(flag.OnAssignmentExpression))
            {
                try
                {
                    database.SaveChanges();
                    var expressionResult = fa.EvaluateOnAssignmentExpression(database, addingUser, fa.AddedDate);
                    if (!string.IsNullOrWhiteSpace(expressionResult))
                    {
                        fa.OnAssignmentExpressionResult = expressionResult;
                        database.SaveChanges();
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
            return ExpressionCache.GetOrCreateSingleExpressions($"UserFlag_OnAssignmentExpression_{uf.Id}", () => Expression.TokenizeSingleDynamic(null, uf.OnAssignmentExpression, 0));
        }

        public static void OnAssignmentExpressionInvalidateCache(this UserFlag uf)
        {
            ExpressionCache.InvalidateSingleCache($"UserFlag_OnAssignmentExpression_{uf.Id}");
        }

        public static string EvaluateOnAssignmentExpression(this UserFlagAssignment ufa, DiscoDataContext Database, User AddingUser, DateTime TimeStamp)
        {
            if (!string.IsNullOrEmpty(ufa.UserFlag.OnAssignmentExpression))
            {
                Expression compiledExpression = ufa.UserFlag.OnAssignmentExpressionFromCache();
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, AddingUser, TimeStamp, null, ufa.User);
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
            return ExpressionCache.GetOrCreateSingleExpressions($"UserFlag_OnUnassignmentExpression_{uf.Id}", () => Expression.TokenizeSingleDynamic(null, uf.OnUnassignmentExpression, 0));
        }

        public static void OnUnassignmentExpressionInvalidateCache(this UserFlag uf)
        {
            ExpressionCache.InvalidateSingleCache($"UserFlag_OnUnassignmentExpression_{uf.Id}");
        }

        public static string EvaluateOnUnassignmentExpression(this UserFlagAssignment ufa, DiscoDataContext Database, User RemovingUser, DateTime TimeStamp)
        {
            if (!string.IsNullOrEmpty(ufa.UserFlag.OnUnassignmentExpression))
            {
                Expression compiledExpression = ufa.UserFlag.OnUnassignmentExpressionFromCache();
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, RemovingUser, TimeStamp, null, ufa.User);
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
