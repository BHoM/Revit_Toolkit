using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Elements;
using BH.oM.Structural.Elements;
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

            if (!Query.IsAssignableFromByFullName(Type, typeof(BHoMObject)))
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

            if (Type == typeof(PanelPlanar))
            {
                aResult.Add(typeof(Floor));
                aResult.Add(typeof(Wall));
                return aResult;
            }
            
            if (Type == typeof(Bar))
            {
                aResult.Add(typeof(FamilyInstance));
                return aResult;
            }

            if (Type == typeof(Building))
            {
                aResult.Add(typeof(SiteLocation));
                return aResult;
            }

            if (Type == typeof(oM.Structural.Elements.Storey))
            {
                aResult.Add(typeof(Level));
                return aResult;
            }

            if (Type == typeof(Space))
            {
                aResult.Add(typeof(SpatialElement));
                return aResult;
            }

            if (Type == typeof(oM.Architecture.Elements.Grid))
            {
                aResult.Add(typeof(Grid));
                return aResult;
            }

            return null;
        }

        /***************************************************/
    }
}
