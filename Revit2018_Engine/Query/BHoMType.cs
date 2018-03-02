using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Properties;
using BH.oM.Environmental.Elements;

namespace BH.Engine.Revit
{
    /***************************************************/
    /**** Public Methods                            ****/
    /***************************************************/

    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Gets BHoM class type for given Element
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <returns name="Type">BHoM class Type</returns>
        /// <search>
        /// Query, BHoM, GetType,  BHoMObject, Revit, Element, Get Type
        /// </search>
        public static Type BHoMType(this Element element)
        {
            if (element is CeilingType)
                return typeof(BuildingElementProperties);
            if (element is WallType)
                return typeof(BuildingElementProperties);
            if (element is FloorType)
                return typeof(BuildingElementProperties);
            if (element is RoofType)
                return typeof(BuildingElementProperties);
            if (element.GetType().IsAssignableFromByFullName(typeof(SpatialElement)))
                return typeof(Space);
            if (element is Wall)
                return typeof(BuildingElement);
            if (element is Ceiling)
                return typeof(BuildingElement);
            if (element.GetType().IsAssignableFromByFullName(typeof(RoofBase)))
                return typeof(BuildingElement);
            if (element is Floor)
                return typeof(BuildingElement);
            if (element is SiteLocation)
                return typeof(Building);
            if (element is Level)
                return typeof(oM.Structural.Elements.Storey);

            return null;
        }
    }

    /***************************************************/
}
