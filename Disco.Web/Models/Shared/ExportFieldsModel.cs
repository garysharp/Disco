using Disco.Data.Repository;
using Disco.Models.Services.Exporting;
using Disco.Models.UI.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Disco.Web.Models.Shared
{
    public static class ExportFieldsModel
    {
        public static ExportFieldsModel<V> Create<V>(V options, V defaultOptions, params string[] ignoreProperties)
            where V : IExportOptions
        {
            return new ExportFieldsModel<V>(options, defaultOptions, ignoreProperties);
        }
    }

    public class ExportFieldsModel<T> : SharedExportFieldsModel<T> where T : IExportOptions
    {
        public T Options { get; set; }
        public List<ExportOptionGroup> FieldGroups { get; set; }

        public ExportFieldsModel(T options, T defaultOptions, params string[] ignoreProperties)
        {
            Options = options;
            FieldGroups = GetFields(options, defaultOptions, ignoreProperties);
        }

        private static List<ExportOptionGroup> GetFields(T options, T defaultOptions, params string[] ignoreProperties)
        {
            var properties = new List<ExportOptionField>();

            foreach (var prop in typeof(T).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (prop.PropertyType != typeof(bool) || ignoreProperties.Contains(prop.Name))
                    continue;

                var display = prop.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                if (display == null)
                    continue;

                properties.Add(new ExportOptionField()
                {
                    GroupName = display.GroupName,
                    Name = prop.Name,
                    DisplayName = display.Name,
                    Description = display.Description,
                    IsDefault = (bool)prop.GetValue(defaultOptions),
                    IsChecked = (bool)prop.GetValue(options),
                });
            }

            return properties
                .GroupBy(p => p.GroupName)
                .Select(g =>
                {
                    var group = new ExportOptionGroup(g.Key);
                    group.AddRange(g);
                    return group;
                })
                .ToList();
        }

        public void AddCustomUserDetails(Expression<Func<T, List<string>>> modelAccessor, int groupIndex = -1)
        {
            List<string> userCustomDetailKeys;
            using (var database = new DiscoDataContext())
                userCustomDetailKeys = database.UserDetails.Where(d => d.Scope == "Details").Select(d => d.Key).Distinct().OrderBy(k => k).ToList();

            if (userCustomDetailKeys.Any())
            {
                var fieldKey = ((MemberExpression)modelAccessor.Body).Member.Name;
                var checkedKeys = modelAccessor.Compile().Invoke(Options);

                var group = new ExportOptionGroup("User Custom Details");
                foreach (var key in userCustomDetailKeys)
                {
                    var displayName = key.TrimEnd('*', '&');
                    group.Add(new ExportOptionField()
                    {
                        GroupName = group.Name,
                        Name = key,
                        DisplayName = displayName,
                        Description = $"{displayName} custom detail for the user",
                        IsChecked = checkedKeys?.Contains(key) ?? false,
                        CustomKey = fieldKey,
                        CustomValue = key,
                    });
                }

                if (groupIndex < 0)
                    FieldGroups.Add(group);
                else
                    FieldGroups.Insert(groupIndex, group);
            }
        }
    }
}
