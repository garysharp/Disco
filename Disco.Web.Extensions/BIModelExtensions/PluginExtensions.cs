using Disco.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Extensions
{
    public static class DiscoPluginDefinitionExtensions
    {
        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginFeatureManifest> PluginFeatureDefinitions, PluginFeatureManifest SelectedItem)
        {
            return PluginFeatureDefinitions.ToSelectListItems(SelectedItem?.Id, false, null);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginFeatureManifest> PluginFeatureDefinitions, IEnumerable<PluginFeatureManifest> SelectedItems)
        {
            return PluginFeatureDefinitions.ToSelectListItems(SelectedItems?.Select(i => i.Id), false, null);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginFeatureManifest> PluginDefinitions, string SelectedId = null, bool IncludeInstructionFirst = false, string InstructionMessage = "Select a Plugin")
        {
            return ToSelectListItems(PluginDefinitions, SelectedId, IncludeInstructionFirst, InstructionMessage, null);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginFeatureManifest> PluginDefinitions, IEnumerable<string> SelectedIds = null, bool IncludeInstructionFirst = false, string InstructionMessage = "Select a Plugin")
        {
            return ToSelectListItems(PluginDefinitions, SelectedIds, IncludeInstructionFirst, InstructionMessage, null);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginFeatureManifest> PluginDefinitions, string SelectedId = null, bool IncludeInstructionFirst = false, string InstructionMessage = "Select a Plugin", Dictionary<string, string> AdditionalItems = null)
        {
            string[] selectedIds = null;
            if (SelectedId != null)
            {
                selectedIds = new string[] { SelectedId };
            }

            return ToSelectListItems(PluginDefinitions, selectedIds, IncludeInstructionFirst, InstructionMessage, AdditionalItems);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginFeatureManifest> PluginDefinitions, IEnumerable<string> SelectedIds = null, bool IncludeInstructionFirst = false, string InstructionMessage = "Select a Plugin", Dictionary<string, string> AdditionalItems = null)
        {
            var selectedIds = SelectedIds?.ToList();

            var items = PluginDefinitions
                .Select(wpd => new SelectListItem { Value = wpd.Id, Text = wpd.Name, Selected = (selectedIds?.Contains(wpd.Id, StringComparer.Ordinal) ?? false) });

            if (AdditionalItems != null)
                items = items.Concat(AdditionalItems.Select(i => new SelectListItem { Value = i.Key, Text = i.Value, Selected = (selectedIds?.Contains(i.Key, StringComparer.Ordinal) ?? false) }));

            var selectItems = items.OrderBy(i => i.Text).ToList();

            if (IncludeInstructionFirst)
                selectItems.Insert(0, new SelectListItem() { Value = string.Empty, Text = $"<{InstructionMessage}>", Selected = (selectedIds?.Count ?? 0) != 0 });

            return selectItems;
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginManifest> PluginFeatureDefinitions, PluginManifest SelectedItem)
        {
            var selectedId = default(string);

            if (SelectedItem != null)
                selectedId = SelectedItem.Id;

            return PluginFeatureDefinitions.ToSelectListItems(selectedId);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginManifest> PluginDefinitions, string SelectedId = null, bool IncludeInstructionFirst = false, string InstructionMessage = "Select a Plugin")
        {
            var selectItems = default(List<SelectListItem>);
            if (SelectedId == null)
                selectItems = PluginDefinitions.Select(wpd => new SelectListItem { Value = wpd.Id, Text = wpd.Name }).ToList();
            else
                selectItems = PluginDefinitions.Select(wpd => new SelectListItem { Value = wpd.Id, Text = wpd.Name, Selected = (SelectedId.Equals(wpd.Id)) }).ToList();

            if (IncludeInstructionFirst)
                selectItems.Insert(0, new SelectListItem() { Value = string.Empty, Text = $"<{InstructionMessage}>", Selected = string.IsNullOrEmpty(SelectedId) });

            return selectItems;
        }
    }
}
