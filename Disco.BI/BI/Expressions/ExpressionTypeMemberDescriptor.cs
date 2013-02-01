using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Disco.BI.Expressions
{
	public class ExpressionTypeMemberDescriptor
	{
		public const string FunctionKind = "function";
		public const string PropertyKind = "property";
		public const string ParameterKind = "parameter";
		
		public string Kind {get;set;}
		public string Name {get;set;}
		public string ReturnType {get;set;}
		public string ReturnExpressionType{get;set;}
		public List<ExpressionTypeMemberDescriptor> Parameters{get;set;}
		
		public static ExpressionTypeMemberDescriptor Build(System.Reflection.MethodInfo m)
		{
			ExpressionTypeMemberDescriptor md = new ExpressionTypeMemberDescriptor
			{
				Kind = "function", 
				Name = m.Name, 
				ReturnType = m.ReturnType.Name, 
				ReturnExpressionType = m.ReturnType.AssemblyQualifiedName
			};
			md.Parameters = (
				from mdp in m.GetParameters()
				select ExpressionTypeMemberDescriptor.Build(mdp)).ToList<ExpressionTypeMemberDescriptor>();
			return md;
		}
		public static ExpressionTypeMemberDescriptor Build(System.Reflection.PropertyInfo p)
		{
			ExpressionTypeMemberDescriptor md = new ExpressionTypeMemberDescriptor
			{
				Kind = "property", 
				Name = p.Name, 
				ReturnType = p.PropertyType.Name, 
				ReturnExpressionType = p.PropertyType.AssemblyQualifiedName
			};
			md.Parameters = (
				from mdp in p.GetIndexParameters()
				select ExpressionTypeMemberDescriptor.Build(mdp)).ToList<ExpressionTypeMemberDescriptor>();
			return md;
		}
		public static ExpressionTypeMemberDescriptor Build(System.Reflection.ParameterInfo pi)
		{
			return new ExpressionTypeMemberDescriptor
			{
				Kind = "parameter", 
				Name = pi.Name, 
				ReturnType = pi.ParameterType.Name, 
				ReturnExpressionType = pi.ParameterType.AssemblyQualifiedName
			};
		}
	}
}
