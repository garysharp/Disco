using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Disco.Services.Expressions
{
    public class ExpressionTypeDescriptor
    {
        public string ExpressionType { get; set; }
        public string Name { get; set; }
        public List<ExpressionTypeMemberDescriptor> Members { get; set; }

        public static ExpressionTypeDescriptor Build(Type t, bool StaticDeclaredMembersOnly = true)
        {
            ExpressionTypeDescriptor i = new ExpressionTypeDescriptor
            {
                ExpressionType = t.AssemblyQualifiedName,
                Name = t.Name
            };
            i.Members = new List<ExpressionTypeMemberDescriptor>();

            MemberInfo[] members;
            if (StaticDeclaredMembersOnly)
                members = t.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            else
                members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance);

            for (int j = 0; j < members.Length; j++)
            {
                MemberInfo member = members[j];
                if (member is PropertyInfo)
                {
                    PropertyInfo pi = (PropertyInfo)member;
                    if (!pi.IsSpecialName && pi.CanRead)
                    {
                        i.Members.Add(ExpressionTypeMemberDescriptor.Build(pi));
                    }
                }
                if (member is MethodInfo)
                {
                    MethodInfo mi2 = (MethodInfo)member;
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
