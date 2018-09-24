using System;
using System.Reflection;

using BH.oM.Adapters.Revit.Elements;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

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

