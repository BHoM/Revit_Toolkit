using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Elements;
using BH.oM.Base;

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
        /// Get Revit class types from BHoM class type.
        /// </summary>
        /// <param name="Type">BHoM class Type</param>
        /// <returns name="Types">Revit class Types</returns>
        /// <search>
        /// Query, RevitTypes, Revit, Get Revit Types, Type, BHoM Type
        /// </search>
        public static IEnumerable<System.Type> RevitTypes(System.Type Type)
        {
            if (Type == null)
                return null;

            if (!Query.IsAssignableFromByFullName(typeof(BHoMObject), Type))
                return null;

            List<System.Type> aResult = new List<System.Type>();
            if (Type == typeof(BuildingElement))
            {
                aResult.Add(typeof(Floor));
                aResult.Add(typeof(Wall));
                aResult.Add(typeof(Ceiling));
                aResult.Add(typeof(RoofBase));
                return aResult;
            }

            return null;
        }

        /***************************************************/
    }
}
