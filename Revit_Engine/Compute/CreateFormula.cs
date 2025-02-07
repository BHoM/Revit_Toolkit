/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Text;

namespace BH.Engine.Adapters.Revit
{
	public static partial class Compute
	{
		/***************************************************/
		/****              Public methods               ****/
		/***************************************************/

		[Description("Creates a delegate from a formula string with specified parameters and return type.")]
		[Input("parametersDeclaration", "A string representing the parameters of the formula.")]
		[Input("returnType", "The return type of the formula.")]
		[Input("formulaString", "The formula string to be compiled into a delegate.")]
		[Input("formulaName", "The name of the formula method for reuse through reflection. Defaults to 'anonymous'.")]
        [Input("blockCode", "A block of code to be included in the compiled method, for complex function use case.")]
        [Output("Delegate", "A delegate representing the compiled formula.")]

		public static Delegate CreateFormula(string parametersDeclaration, string returnType, string formulaString, string formulaName = "anonymous")
		{
			Assembly[] assemblies = new Assembly[]
			{
				Assembly.GetAssembly(typeof(System.Math)),
				Assembly.GetAssembly(typeof(Enumerable)),
				Assembly.GetAssembly(typeof(Compute)),
			};

			var assemblyPaths = assemblies
				.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
				.Select(a => a.Location)    
				.Distinct()
				.Select(loc => MetadataReference.CreateFromFile(loc))
				.ToList();

			// Wrap user code in a class and method
            string codeString = CodeTemplate(parametersDeclaration, returnType, formulaString, formulaName);
            if (codeString == null)
            {
                return null;
            }

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeString);
			CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName: "RuntimeAssembly_" + Guid.NewGuid().ToString("N"),
				syntaxTrees: new[] { syntaxTree },
				references: assemblyPaths,
				options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			);

			using (var ms = new MemoryStream())
			{
				EmitResult result = compilation.Emit(ms);

				if (!result.Success)
				{
					StringBuilder err = new StringBuilder("Failed, Error :");
					err.AppendLine($"params : {parametersDeclaration}");
                    // If compilation failed, print errors
                    IEnumerable<Diagnostic> failures =
						result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
					foreach (Diagnostic diag in failures)
					{
						//Console.WriteLine($"{diag.Id}: {diag.GetMessage()}");
						err.AppendLine($"{diag.Id}: {diag.GetMessage()}");
					}
					err.Replace("System.", "");
                    if (err.Length > 255)
                    {
                        err.Length = 252;
                        err.Append("...");
                    }
                    BH.Engine.Base.Compute.RecordError(err.ToString());
					return null;

				}

				ms.Seek(0, SeekOrigin.Begin);
				Assembly assembly = Assembly.Load(ms.ToArray());

				Type type = assembly.GetType("CustomMethodsClass");
				if (type == null)
				{
                    BH.Engine.Base.Compute.RecordError("Type 'CustomMethodsClass' not found in the assembly.");
                    return null;
                }

				MethodInfo method = type.GetMethod(formulaName, BindingFlags.Public | BindingFlags.Static);
				if (method == null)
				{
                    BH.Engine.Base.Compute.RecordError($"Method {formulaName} not found in the type 'CustomMethodsClass'.");
                    return null;
                }

				var argumentTypes = method.GetParameters().Select(p => p.ParameterType).Append(method.ReturnType).ToArray();

				Type delegateType = Expression.GetFuncType(argumentTypes);

				return Delegate.CreateDelegate(delegateType, method);
			}
		}

		[Description("Creates a delegate from a formula string with specified parameters and return type.")]
		[Input("parameterFormula", "A ParameterFormula object containing input RevitParameters, return type, and formula string.")]
		[Input("formulaName", "The name of the formula method. Default to GUID string.")]
		[Output("Delegate", "A delegate representing the compiled formula.")]

		public static Delegate CreateFormula(ParameterFormula parameterFormula, string formulaName = null)
		{

			string parametersDeclaration = string.Join(", ", parameterFormula.InputParameters.Select(p => p.Value.GetType().FullName + " " + p.Name));

			if (parameterFormula.ExternalData != null)
			{
				foreach (var data in parameterFormula.ExternalData)
				{
					string dataType;
					if (data.Value is List<List<object>>)
					{
						dataType = "List<List<object>>";
					}
					else if (data.Value is List<object>)
					{
						dataType = "List<object>";
					}
					else
					{
						dataType = data.Value.GetType().FullName;
					}
					parametersDeclaration += $", {dataType} {data.Key}";
				}
			}

			string formulaKey = ComputeFormulaKey(parametersDeclaration, parameterFormula.ReturnType, parameterFormula.Formula);

			if (m_cachedFormula.ContainsKey(formulaKey))
			{
				return m_cachedFormula[formulaKey];
			}

			formulaName = formulaName ?? "F" + Guid.NewGuid().ToString().Replace('-','_');

			Delegate formula = CreateFormula(
				parametersDeclaration, 
				parameterFormula.ReturnType, 
				parameterFormula.Formula,
				formulaName);

			if (formula == null)
			{
				return null;
            }
			m_cachedFormula.GetOrAdd(formulaKey, formula);
			return formula;
		}

		/***************************************************/
		/****              Private methods              ****/
		/***************************************************/

		/// <summary>
		/// This Template allows to use all methods from math enumerable types, and custom methods from BH.Engine.Adapters.Revit.Compute
		/// </summary>
		private static string CodeTemplate(string parametersDeclaration, string returnType, string formulaString, string formulaName)
		{
			char[] specialChars = {';', '\n', '{' };
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "if", "case", "else", "while", "for", "foreach", "switch", "try", "catch", "finally", "do", "break", "continue", "return", "goto", "throw", "using", "lock", "checked", "unchecked", "fixed", "unsafe", "default", "delegate", "event", "explicit", "implicit", "namespace", "operator", "params", "partial", "private", "protected", "public", "readonly", "sealed", "static", "this", "base", "new", "as", "is", "sizeof", "typeof", "stackalloc", "checked", "unchecked", "virtual", "abstract", "override", "extern", "ref", "out", "in", "object", "string", "int", "float", "double", "decimal", "bool", "char", "byte", "sbyte", "short", "ushort", "uint", "long", "ulong", "void", "null", "true", "false", "class", "struct", "interface", "enum", "void", "using", "get", "set", "add", "remove", "value", "alias", "global", "checked", "unchecked", "fixed", "unsafe", "implicit", "explicit", "operator", "params", "ref", "out", "in", "is", "as", "sizeof", "typeof", "stackalloc", "delegate", "event", "object", "string", "int", "float", "double", "decimal", "bool", "char", "byte", "sbyte", "short", "ushort", "uint", "long", "ulong", "void", "null", "true", "false", "class", "struct", "interface", "enum", "void", "using", "get", "set", "add", "remove", "value", "alias", "global", "checked", "unchecked", "fixed", "unsafe", "implicit", "explicit", "operator", "params", "ref", "out", "in", "is", "as", "sizeof", "typeof", "stackalloc", "delegate", "event", "object", "string", "int", "float", "double", "decimal", "bool", "char", "byte", "sbyte", "short", "ushort" };

            bool containsSpecialChars = formulaString.IndexOfAny(specialChars) >= 0;
            bool containsKeyword = keywords.Any(keyword => Regex.IsMatch(formulaString, $@"\b{Regex.Escape(keyword)}\b"));
            bool isComplexFormula = containsSpecialChars || containsKeyword;

            string blockCode;
            if (isComplexFormula)
            {
                blockCode = formulaString;
				if (!Regex.IsMatch(blockCode, @"\breturn\b"))
				{
                    BH.Engine.Base.Compute.RecordError("Syntax Error: Complex formula must contain a return statement.");
                    return null;
				}
				
				formulaString = string.Empty;
            }
            else
            {
                blockCode = $"return {formulaString};";
            }
            return

$@"
using System;
using System.Linq;
using System.Collections.Generic;
using static System.Math;
using static System.String;
using static BH.Engine.Adapters.Revit.Compute;

public static class CustomMethodsClass
{{
	public static {returnType} {formulaName}({parametersDeclaration})
	{{
		{blockCode}
	}}
}}";
		}

		private static string ComputeFormulaKey(string parametersDeclaration, string returnType, string formulaString)
		{
			// create a string that uniquely identifies the formula.
			return $"{parametersDeclaration} - {returnType} - {formulaString}";
		}

		/***************************************************/
		/****              Private fields               ****/
		/***************************************************/

		private static ConcurrentDictionary<string, Delegate> m_cachedFormula = new ConcurrentDictionary<string, Delegate>();

		/***************************************************/
	}
}

