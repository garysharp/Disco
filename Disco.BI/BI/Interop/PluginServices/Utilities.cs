//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Disco.Data.Repository;
//using Quartz;

//namespace Disco.BI.Interop.PluginServices
//{
//    public static class Utilities
//    {

//        public static void InitalizeScheduledTasks(DiscoDataContext dbContext, ISchedulerFactory SchedulerFactory)
//        {

//            var scheduler = SchedulerFactory.GetScheduler();

//            // Discover IDiscoScheduledTasks (Only from Disco Assemblies)
//            var appDomain = AppDomain.CurrentDomain;

//            var scheduledTaskTypes = (from a in appDomain.GetAssemblies()
//                                      where !a.GlobalAssemblyCache && !a.IsDynamic && a.FullName.StartsWith("Disco.", StringComparison.InvariantCultureIgnoreCase)
//                                      from type in a.GetTypes()
//                                      where typeof(IDiscoScheduledTask).IsAssignableFrom(type) && !type.IsAbstract
//                                      select type);
//            foreach (Type scheduledTaskType in scheduledTaskTypes)
//            {
//                IDiscoScheduledTask instance = (IDiscoScheduledTask)Activator.CreateInstance(scheduledTaskType);
//                try
//                {
//                    instance.InitalizeScheduledTask(dbContext, scheduler);
//                }
//                catch (Exception ex)
//                {
//                    if (instance == null)
//                        Logging.SystemLog.LogException("Initializing Scheduled Task; Disco.BI.Interop.Plugins.Utilities.InitalizeScheduledTasks()", ex);
//                    else
//                        Logging.SystemLog.LogException(string.Format("Initializing Scheduled Task: '{0}'; Disco.BI.Interop.Plugins.Utilities.InitalizeScheduledTasks()", instance.GetType().Name), ex);
//                }
//            }

//        }

//    }
//}
