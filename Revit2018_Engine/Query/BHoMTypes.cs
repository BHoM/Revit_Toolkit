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
using Autodesk.Revit.DB.Structure.StructuralSections;

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
                switch ((element as FamilyInstance).StructuralType)
                {
                    case StructuralType.Beam:
                    case StructuralType.Column:
                    case StructuralType.Brace:
                        aResult.Add(typeof(Bar));
                        return aResult;
                }

                //Environmental Windows and Doors
                switch((BuiltInCategory)element.Category.Id.IntegerValue)
                {
                    case BuiltInCategory.OST_Windows:
                    case BuiltInCategory.OST_Doors:
                        aResult.Add(typeof(BuildingElement));
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
                aResult.Add(typeof(Level));
                return aResult;
            }

            if (element is Grid)
            {
                aResult.Add(typeof(oM.Architecture.Elements.Grid));
                return aResult;
            }

            return null; //TODO: shouldn't it be aResult?
        }

        /***************************************************/

        public static List<Type> BHoMTypes(this StructuralSectionShape sectionShape)
        {
            int shapeNum = (int)sectionShape;
            List<Type> aResult = new List<Type>();

            if (shapeNum == 2)
            {
                aResult.Add(typeof(RectangleSectionDimensions));
                return aResult;
            }

            if (shapeNum == 5)
            {
                aResult.Add(typeof(TubeDimensions));
                return aResult;
            }

            if (shapeNum == 6)
            {
                aResult.Add(typeof(StandardISectionDimensions));
                return aResult;
            }

            if (shapeNum == 8)
            {
                aResult.Add(typeof(StandardISectionDimensions));
                return aResult;
            }

            if (shapeNum == 9)
            {
                aResult.Add(typeof(StandardChannelSectionDimensions));
                return aResult;
            }

            if (shapeNum == 11)
            {
                aResult.Add(typeof(StandardAngleSectionDimensions));
                return aResult;
            }

            if (shapeNum == 12)
            {
                aResult.Add(typeof(RectangleSectionDimensions));
                return aResult;
            }

            if (shapeNum == 13)
            {
                aResult.Add(typeof(CircleDimensions));
                return aResult;
            }

            if (shapeNum == 14)
            {
                aResult.Add(typeof(StandardBoxDimensions));
                return aResult;
            }

            if (shapeNum == 15)
            {
                aResult.Add(typeof(TubeDimensions));
                return aResult;
            }

            if (shapeNum == 16)
            {
                aResult.Add(typeof(FabricatedISectionDimensions));
                return aResult;
            }

            if (shapeNum == 17)
            {
                aResult.Add(typeof(StandardTeeSectionDimensions));
                return aResult;
            }

            if (shapeNum == 19)
            {
                aResult.Add(typeof(StandardTeeSectionDimensions));
                return aResult;
            }

            if (shapeNum == 20)
            {
                aResult.Add(typeof(StandardChannelSectionDimensions));
                return aResult;
            }

            if (shapeNum == 23)
            {
                aResult.Add(typeof(StandardAngleSectionDimensions));
                return aResult;
            }

            if (shapeNum == 25)
            {
                aResult.Add(typeof(StandardZedSectionDimensions));
                return aResult;
            }

            if (shapeNum == 31)
            {
                aResult.Add(typeof(RectangleSectionDimensions));
                return aResult;
            }

            if (shapeNum == 33)
            {
                aResult.Add(typeof(StandardTeeSectionDimensions));
                return aResult;
            }

            if (shapeNum == 35)
            {
                aResult.Add(typeof(CircleDimensions));
                return aResult;
            }

            return aResult;
        }

        /***************************************************/
    }
}