using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
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

        #region Edit Comments
        public static bool CanEditComments(this DeviceFlagAssignment fa)
        {
            return UserService.CurrentAuthorization.Has(Claims.Device.Actions.EditFlags);
        }
        public static void OnEditComments(this DeviceFlagAssignment fa, string Comments)
        {
            if (!fa.CanEditComments())
                throw new InvalidOperationException("Editing comments for device flags is denied");

            fa.Comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim();
        }
        #endregion

        #region Remove
        public static bool CanRemove(this DeviceFlagAssignment fa)
        {
            if (fa.RemovedDate.HasValue)
                return false;

            return UserService.CurrentAuthorization.Has(Claims.Device.Actions.RemoveFlags);
        }
        public static void OnRemove(this DeviceFlagAssignment fa, DiscoDataContext Database, User RemovingUser)
        {
            if (!fa.CanRemove())
                throw new InvalidOperationException("Removing device flags is denied");

            fa.OnRemoveUnsafe(Database, RemovingUser);
        }

        public static void OnRemoveUnsafe(this DeviceFlagAssignment fa, DiscoDataContext Database, User RemovingUser)
        {
            fa = Database.DeviceFlagAssignments
                .Include(a => a.DeviceFlag)
                .First(a => a.Id == fa.Id);
            RemovingUser = Database.Users.First(u => u.UserId == RemovingUser.UserId);

            fa.RemovedDate = DateTime.Now;
            fa.RemovedUserId = RemovingUser.UserId;

            if (!string.IsNullOrWhiteSpace(fa.DeviceFlag.OnUnassignmentExpression))
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
                    SystemLog.LogException("Device Flag Expression - OnUnassignmentExpression", ex);
                }
            }
        }
        #endregion

        #region Add
        public static bool CanAddDeviceFlags(this Device d)
        {
            return UserService.CurrentAuthorization.Has(Claims.Device.Actions.AddFlags);
        }
        public static bool CanAddDeviceFlag(this Device d, DeviceFlag flag)
        {
            // Shortcut
            if (!d.CanAddDeviceFlags())
                return false;

            // Already has Device Flag?
            if (d.DeviceFlagAssignments.Any(fa => !fa.RemovedDate.HasValue && fa.DeviceFlagId == flag.Id))
                return false;

            return true;
        }
        public static DeviceFlagAssignment OnAddDeviceFlag(this Device d, DiscoDataContext Database, DeviceFlag flag, User AddingUser, string Comments)
        {
            if (!d.CanAddDeviceFlag(flag))
                throw new InvalidOperationException("Adding device flag is denied");

            return d.OnAddDeviceFlagUnsafe(Database, flag, AddingUser, Comments);
        }

        public static DeviceFlagAssignment OnAddDeviceFlagUnsafe(this Device d, DiscoDataContext Database, DeviceFlag flag, User AddingUser, string Comments)
        {
            flag = Database.DeviceFlags.First(f => f.Id == flag.Id);
            d = Database.Devices.First(de => de.SerialNumber == d.SerialNumber);
            AddingUser = Database.Users.First(user => user.UserId == AddingUser.UserId);

            var fa = new DeviceFlagAssignment()
            {
                DeviceFlag = flag,
                Device = d,
                AddedDate = DateTime.Now,
                AddedUser = AddingUser,
                AddedUserId = AddingUser.UserId,
                Comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim()
            };

            Database.DeviceFlagAssignments.Add(fa);

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
