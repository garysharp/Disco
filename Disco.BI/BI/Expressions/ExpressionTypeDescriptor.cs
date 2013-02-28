using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Disco.BI.Expressions
{
    public class ExpressionTypeDescriptor
    {
        public string ExpressionType { get; set; }
        public string Name { get; set; }
        public List<ExpressionTypeMemberDescriptor> Members { get; set; }

        public static ExpressionTypeDescriptor Build(System.Type t, bool StaticDeclaredMembersOnly = true)
        {
            ExpressionTypeDescriptor i = new ExpressionTypeDescriptor
            {
                ExpressionType = t.AssemblyQualifiedName,
                Name = t.Name
            };
            i.Members = new System.Collections.Generic.List<ExpressionTypeMemberDescriptor>();

            System.Reflection.MemberInfo[] members;
            if (StaticDeclaredMembersOnly)
                members = t.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            else
                members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance);

            for (int j = 0; j < members.Length; j++)
            {
                System.Reflection.MemberInfo member = members[j];
                if (member is System.Reflection.PropertyInfo)
                {
                    System.Reflection.PropertyInfo pi = (System.Reflection.PropertyInfo)member;
                    if (!pi.IsSpecialName && pi.CanRead)
                    {
                        i.Members.Add(ExpressionTypeMemberDescriptor.Build(pi));
                    }
                }
                if (member is System.Reflection.MethodInfo)
                {
                    System.Reflection.MethodInfo mi2 = (System.Reflection.MethodInfo)member;
                    if (!mi2.IsSpecialName)
                    {
                        i.Members.Add(ExpressionTypeMemberDescriptor.Build(mi2));
                    }
                }
            }
            i.Members = (
                from mi in i.Members
                orderby mi.Name
                select mi).ToList();
            return i;
        }
    }
}
