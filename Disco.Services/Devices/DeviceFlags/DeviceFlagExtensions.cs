using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Devices.DeviceFlags;
using Disco.Services.Expressions;
using Disco.Services.Logging;
using Disco.Services.Users;
using System;
using System.Collections;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services
{
    public static class DeviceFlagExtensions
    {

        #region Edit
        public static bool CanEdit(this DeviceFlagAssignment fa)
        {
            var (_, permission) = DeviceFlagService.GetDeviceFlag(fa.DeviceFlagId);

            return permission.CanEdit();
        }
        public static void OnEdit(this DeviceFlagAssignment fa, string comments, DateTime? removeDate)
        {
            if (!fa.CanEdit())
                throw new InvalidOperationException("Editing comments for device flags is denied");

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
        public static bool CanRemove(this DeviceFlagAssignment fa)
        {
            if (fa.RemovedDate.HasValue)
                return false;

            var (_, permission) = DeviceFlagService.GetDeviceFlag(fa.DeviceFlagId);

            return permission.CanRemove();
        }
        public static void OnRemove(this DeviceFlagAssignment fa, DiscoDataContext database)
        {
            if (!fa.CanRemove())
                throw new InvalidOperationException("Removing device flags is denied");

            fa.OnRemoveUnsafe(database, UserService.CurrentUser);
        }

        public static void OnRemoveUnsafe(this DeviceFlagAssignment fa, DiscoDataContext database, User removingUser)
        {
            OnRemoveUnsafe(fa, database, removingUser, isScheduled: false);
        }

        internal static void OnRemoveUnsafe(this DeviceFlagAssignment fa, DiscoDataContext database, User removingUser, bool isScheduled)
        {
            fa = database.DeviceFlagAssignments
                .Include(a => a.DeviceFlag)
                .First(a => a.Id == fa.Id);
            removingUser = database.Users.First(u => u.UserId == removingUser.UserId);

            fa.RemovedDate = DateTime.Now;
            fa.RemovedUserId = removingUser.UserId;

            if (!isScheduled)
            {
                fa.RemoveDate = null;
                fa.RemoveUser = null;
                fa.RemoveUserId = null;
            }

            if (!string.IsNullOrWhiteSpace(fa.DeviceFlag.OnUnassignmentExpression))
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
                    SystemLog.LogException("Device Flag Expression - OnUnassignmentExpression", ex);
                }
            }
        }
        #endregion

        #region Add
        public static bool CanAddDeviceFlag(this Device d, DeviceFlag flag)
        {
            // Already has Device Flag?
            if (d.DeviceFlagAssignments.Any(fa => !fa.RemovedDate.HasValue && fa.DeviceFlagId == flag.Id))
                return false;

            var (_, permission) = DeviceFlagService.GetDeviceFlag(flag.Id);

            return permission.CanAssign();
        }
        public static bool CanRemoveDeviceFlag(this Device d, DeviceFlag flag)
        {
            var (_, permission) = DeviceFlagService.GetDeviceFlag(flag.Id);

            return permission.CanRemove();
        }
        public static DeviceFlagAssignment OnAddDeviceFlag(this Device d, DiscoDataContext database, DeviceFlag flag, string comments)
        {
            if (!d.CanAddDeviceFlag(flag))
                throw new InvalidOperationException("Adding device flag is denied");

            var removeDate = (DateTime?)null;
            if (flag.DefaultRemoveDays.HasValue)
                removeDate = DateTime.Today.AddDays(flag.DefaultRemoveDays.Value);

            return OnAddDeviceFlag(d, database, flag, comments, removeDate);
        }

        public static DeviceFlagAssignment OnAddDeviceFlag(this Device d, DiscoDataContext database, DeviceFlag flag, string comments, DateTime? removeDate)
        {
            if (!d.CanAddDeviceFlag(flag))
                throw new InvalidOperationException("Adding device flag is denied");

            if (d.CanRemoveDeviceFlag(flag))
                return d.OnAddDeviceFlagUnsafe(database, flag, UserService.CurrentUser, comments, removeDate);
            else
                return d.OnAddDeviceFlagUnsafe(database, flag, UserService.CurrentUser, comments);
        }

        public static DeviceFlagAssignment OnAddDeviceFlagUnsafe(this Device d, DiscoDataContext database, DeviceFlag flag, User addingUser, string comments)
        {
            var removeDate = (DateTime?)null;
            if (flag.DefaultRemoveDays.HasValue)
                removeDate = DateTime.Today.AddDays(flag.DefaultRemoveDays.Value);

            return OnAddDeviceFlagUnsafe(d, database, flag, addingUser, comments, removeDate);
        }

        public static DeviceFlagAssignment OnAddDeviceFlagUnsafe(this Device d, DiscoDataContext database, DeviceFlag flag, User addingUser, string comments, DateTime? removeDate)
        {
            flag = database.DeviceFlags.First(f => f.Id == flag.Id);
            d = database.Devices.First(de => de.SerialNumber == d.SerialNumber);
            addingUser = database.Users.First(user => user.UserId == addingUser.UserId);

            var fa = new DeviceFlagAssignment()
            {
                DeviceFlag = flag,
                Device = d,
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

            database.DeviceFlagAssignments.Add(fa);

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
                    SystemLog.LogException("Device Flag Expression - OnAssignmentExpression", ex);
                }
            }

            return fa;
        }
        #endregion

        #region Expressions

        public static Expression OnAssignmentExpressionFromCache(this DeviceFlag uf)
        {
            return ExpressionCache.GetOrCreateSingleExpressions($"DeviceFlag_OnAssignmentExpression_{uf.Id}", () => Expression.TokenizeSingleDynamic(null, uf.OnAssignmentExpression, 0));
        }

        public static void OnAssignmentExpressionInvalidateCache(this DeviceFlag uf)
        {
            ExpressionCache.InvalidateSingleCache($"DeviceFlag_OnAssignmentExpression_{uf.Id}");
        }

        public static string EvaluateOnAssignmentExpression(this DeviceFlagAssignment dfa, DiscoDataContext Database, User AddingUser, DateTime TimeStamp)
        {
            if (!string.IsNullOrEmpty(dfa.DeviceFlag.OnAssignmentExpression))
            {
                Expression compiledExpression = dfa.DeviceFlag.OnAssignmentExpressionFromCache();
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, AddingUser, TimeStamp, null, dfa.Device);
                object result = compiledExpression.EvaluateFirst<object>(dfa, evaluatorVariables);
                if (result == null)
                    return null;
                else
                    return result.ToString();
            }
            return null;
        }

        public static Expression OnUnassignmentExpressionFromCache(this DeviceFlag df)
        {
            return ExpressionCache.GetOrCreateSingleExpressions($"DeviceFlag_OnUnassignmentExpression_{df.Id}", () => Expression.TokenizeSingleDynamic(null, df.OnUnassignmentExpression, 0));
        }

        public static void OnUnassignmentExpressionInvalidateCache(this DeviceFlag df)
        {
            ExpressionCache.InvalidateSingleCache($"DeviceFlag_OnUnassignmentExpression_{df.Id}");
        }

        public static string EvaluateOnUnassignmentExpression(this DeviceFlagAssignment dfa, DiscoDataContext Database, User RemovingUser, DateTime TimeStamp)
        {
            if (!string.IsNullOrEmpty(dfa.DeviceFlag.OnUnassignmentExpression))
            {
                Expression compiledExpression = dfa.DeviceFlag.OnUnassignmentExpressionFromCache();
                IDictionary evaluatorVariables = Expression.StandardVariables(null, Database, RemovingUser, TimeStamp, null, dfa.Device);
                object result = compiledExpression.EvaluateFirst<object>(dfa, evaluatorVariables);
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
