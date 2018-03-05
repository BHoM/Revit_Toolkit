using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using BH.oM.Environmental.Properties;
using BH.oM.Environmental.Elements;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;

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
        /// Gets BHoM class types for given Element
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <returns name="Type">BHoM class Type</returns>
        /// <search>
        /// Query, BHoM, GetTypes,  BHoMObject, Revit, Element, Get Types
        /// </search>
        public static List<Type> BHoMTypes(this Element element)
        {
            List<Type> aResult = new List<Type>();

            if (element is FamilyInstance)
            {
                switch (((FamilyInstance)element).StructuralType)
                {
                    case StructuralType.Beam:
                    case StructuralType.Column:
                    case StructuralType.Brace:
                        aResult.Add(typeof(Bar));
                        return aResult;
                }
            }

            if (element is CeilingType)
            {
                aResult.Add(typeof(BuildingElementProperties));
                return aResult;
            }

            if (element is WallType)
            {
                aResult.Add(typeof(BuildingElementProperties));
                aResult.Add(typeof(Property2D));
                return aResult;
            }

            if (element is FloorType)
            {
                aResult.Add(typeof(BuildingElementProperties));
                aResult.Add(typeof(Property2D));
                return aResult;
            }

            if (element is RoofType)
            {
                aResult.Add(typeof(BuildingElementProperties));
                aResult.Add(typeof(Property2D));
                return aResult;
            }

            if (element.GetType().IsAssignableFromByFullName(typeof(SpatialElement)))
            {
                aResult.Add(typeof(Space));
                return aResult;
            }

            if (element is Wall)
            {
                aResult.Add(typeof(BuildingElement));
                aResult.Add(typeof(PanelPlanar));
                return aResult;
            }

            if (element is Ceiling)
            {
                aResult.Add(typeof(BuildingElement));
                return aResult;
            }


            if (element.GetType().IsAssignableFromByFullName(typeof(RoofBase)))
            {
                aResult.Add(typeof(BuildingElement));
                aResult.Add(typeof(PanelPlanar));
                return aResult;
            }

            if (element is Floor)
            {
                aResult.Add(typeof(BuildingElement));
                aResult.Add(typeof(PanelPlanar));
                return aResult;
            }

            if (element is SiteLocation)
            {
                aResult.Add(typeof(Building));
                return aResult;
            }

            if (element is Level)
            {
                aResult.Add(typeof(Storey));
                return aResult;
            }

            return null;
        }
    }

    /***************************************************/
}
