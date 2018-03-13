using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Parameter LookupParameter(this Element element, IEnumerable<string> parameterNames)
        {
            foreach (string s in parameterNames)
            {
                Parameter p= element.LookupParameter(s);
                if (p != null && p.HasValue) return p;
            }
            return null;
        }

        /***************************************************/

        public static double LookupParameterDouble(this Element element, string parameterName, bool convertToSI = false)
        {
            double value = double.NaN;
            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertToSI) value *= UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_METERS);
            }
            return value;
        }

        /***************************************************/

        public static double LookupParameterDouble(this Element element, IEnumerable<string> parameterNames, bool convertToSI = false)
        {
            double value = double.NaN;
            Parameter p = element.LookupParameter(parameterNames);
            if (p != null)
            {
                value = p.AsDouble();
                if (convertToSI) value *= UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_METERS);
            }
            return value;
        }

        /***************************************************/
    }
}