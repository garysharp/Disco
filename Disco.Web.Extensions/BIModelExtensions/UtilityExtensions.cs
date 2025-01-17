﻿using System;

namespace Disco.Web.Extensions
{
    public static class UtilityExtensions
    {
        public static string ToJavascriptDate(this DateTime d)
        {
            return string.Format("new Date({0}, {1}, {2}, {3}, {4}, {5})", d.Year, d.Month - 1, d.Day, d.Hour, d.Minute, d.Second);
        }
        public static string ToJavascriptDate(this DateTime? d, DateTime? DefaultDate = null)
        {
            if (d.HasValue)
            {
                return ToJavascriptDate(d.Value);
            }
            else
            {
                if (DefaultDate.HasValue)
                    return ToJavascriptDate(DefaultDate.Value);
                else
                    return "null";
            }
        }
    }
}
