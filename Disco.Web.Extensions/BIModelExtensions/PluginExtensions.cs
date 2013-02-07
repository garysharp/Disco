using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Disco.Services.Plugins;

namespace Disco.Web.Extensions
{
    public static class DiscoPluginDefinitionExtensions
    {
        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginFeatureManifest> PluginFeatureDefinitions, PluginFeatureManifest SelectedItem)
        {
            string selectedId = default(string);

            if (SelectedItem != null)
                selectedId = SelectedItem.Id;

            return PluginFeatureDefinitions.ToSelectListItems(selectedId);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginFeatureManifest> PluginDefinitions, string SelectedId = null, bool IncludeInstructionFirst = false, string InstructionMessage = "Select a Plugin")
        {
            var selectItems = default(List<SelectListItem>);
            if (SelectedId == null)
                selectItems =  PluginDefinitions.Select(wpd => new SelectListItem { Value = wpd.Id, Text = wpd.Name }).ToList();
            else
                selectItems = PluginDefinitions.Select(wpd => new SelectListItem { Value = wpd.Id, Text = wpd.Name, Selected = (SelectedId.Equals(wpd.Id)) }).ToList();

            if (IncludeInstructionFirst)
                selectItems.Insert(0, new SelectListItem() { Value = String.Empty, Text = String.Format("<{0}>", InstructionMessage), Selected = String.IsNullOrEmpty(SelectedId) });

            return selectItems;
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<PluginManifest> PluginFeatureDefinitions, PluginManifest SelectedItem)
        {
            string selectedId = default(string);

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
                selectItems.Insert(0, new SelectListItem() { Value = String.Empty, Text = String.Format("<{0}>", InstructionMessage), Selected = String.IsNullOrEmpty(SelectedId) });

            return selectItems;
        }
    }
}
