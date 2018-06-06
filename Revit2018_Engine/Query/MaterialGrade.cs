using System.Collections.Generic;

using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Linq;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Gets Material Grade of Revit element
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <returns name="MaterialGrade">Material Grade</returns>
        /// <search>
        /// Query, Material Grade, Revit, MaterialGrade, element
        /// </search>
        static public string MaterialGrade(this Element element)
        {
            if (element == null)
                return null;

            Parameter aParameter = element.LookupParameter("BHE_Material Grade");
            if (aParameter == null)
                return null;

            return aParameter.AsString();
        }

        /***************************************************/
    }
}

