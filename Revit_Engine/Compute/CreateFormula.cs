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
        [Input("formulaName", "The name of the formula method. Defaults to 'anonymous'.")]
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
                    // If compilation failed, print errors
                    IEnumerable<Diagnostic> failures =
                        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                    foreach (Diagnostic diag in failures)
                    {
                        Console.WriteLine($"{diag.Id}: {diag.GetMessage()}");
                    }
                    throw new Exception("Roslyn compilation failed.");
                }

                // If success, load the assembly from the stream
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                // Retrieve our class and method via reflection
                Type type = assembly.GetType("CustomMethodsClass");
                if (type == null)
                {
                    throw new Exception("Type 'MyRuntimeClass' not found in the assembly.");
                }

                MethodInfo method = type.GetMethod(formulaName, BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    throw new Exception($"Method {formulaName} not found in the type 'CustomMethodsClass'.");
                }

                var argumentTypes = method.GetParameters().Select(p => p.ParameterType).Append(method.ReturnType).ToArray();

                Type delegateType = Expression.GetFuncType(argumentTypes);

                return Delegate.CreateDelegate(delegateType, method);
            }
        }

        [Description("Creates a delegate from a formula string with specified parameters and return type.")]
        [Input("parameterFormula", "A ParameterFormula object containing input RevitParameters, return type, and formula string.")]
        [Input("formulaName", "The name of the formula method. Defaults to 'anonymous'.")]
        [Output("Delegate", "A delegate representing the compiled formula.")]

        public static Delegate CreateFormula(ParameterFormula parameterFormula, string formulaName = "anonymous")
        {

            string formulaKey = ComputeFormulaKey(parameterFormula);

            if (m_cachedFormula.ContainsKey(formulaKey))
            {
                return m_cachedFormula[formulaKey];
            }

            string parametersDeclaration = string.Join(", ", parameterFormula.InputParameters.Select(p => p.Value.GetType().FullName + " " + p.Name));

            if (parameterFormula.CustomData != null)
            {
                foreach (var data in parameterFormula.CustomData)
                {
                    parametersDeclaration += $", {data.Value.GetType().FullName} {data.Key}";
                }
            }

            Delegate formula = CreateFormula(parametersDeclaration, parameterFormula.ReturnType, parameterFormula.Formula, formulaName);

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
            return 
    $@"
    using System;
    using System.Linq;
    using static System.Math;
    using static System.String;
    using static BH.Engine.Adapters.Revit.Compute;

    public static class CustomMethodsClass
    {{
        public static {returnType} {formulaName}({parametersDeclaration})
        {{
            return {formulaString};
        }}
    }}";
        }

        private static string ComputeFormulaKey(ParameterFormula formula)
        {
            // the goal is to create a string that uniquely identifies the formula.
            return $"{formula.ReturnType}-{formula.Formula}-{string.Join(",", formula.InputParameters.Select(p => p.Name))}";
        }

        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static ConcurrentDictionary<string, Delegate> m_cachedFormula = new ConcurrentDictionary<string, Delegate>();

        /***************************************************/
    }
}

