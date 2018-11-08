using System;
using System.Reflection;
using System.ComponentModel;

using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets current domain assembly for given Manifest Module name.")]
        [Input("manifestModuleName", "Manifest Module Name")]
        [Output("Assembly")]
        public static Assembly CurrentDomainAssembly(this string manifestModuleName)
        {
            foreach (Assembly aAssembly in AppDomain.CurrentDomain.GetAssemblies())
                if (aAssembly.ManifestModule.Name == manifestModuleName)
                    return aAssembly;

            return null;
        }

        /***************************************************/
    }
}

