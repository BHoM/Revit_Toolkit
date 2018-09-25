using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Structure.Elements;


namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IEnumerable<System.Type> RevitTypes(System.Type type)
        {
            if (type == null)
                return null;

            if (!BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(BHoMObject)))
                return null;

            List<System.Type> aResult = new List<System.Type>();
            if (type == typeof(BuildingElement))
            {
                aResult.Add(typeof(Floor));
                aResult.Add(typeof(Wall));
                aResult.Add(typeof(Ceiling));
                aResult.Add(typeof(RoofBase));
                aResult.Add(typeof(FamilyInstance));
                return aResult;
            }

            if (type == typeof(PanelPlanar))
            {
                aResult.Add(typeof(Floor));
                aResult.Add(typeof(Wall));
                return aResult;
            }
            
            if (type == typeof(FramingElement))
            {
                aResult.Add(typeof(FamilyInstance));
                return aResult;
            }

            if (type == typeof(Building))
            {
                aResult.Add(typeof(ProjectInfo));
                return aResult;
            }

            if (type == typeof(oM.Architecture.Elements.Level))
            {
                aResult.Add(typeof(Level));
                return aResult;
            }

            if (type == typeof(Space))
            {
                aResult.Add(typeof(SpatialElement));
                return aResult;
            }

            if (type == typeof(oM.Architecture.Elements.Grid))
            {
                aResult.Add(typeof(Grid));
                return aResult;
            }

            if (type == typeof(Sheet))
            {
                aResult.Add(typeof(ViewSheet));
                return aResult;
            }

            if (type == typeof(oM.Adapters.Revit.Elements.ViewPlan))
            {
                aResult.Add(typeof(Autodesk.Revit.DB.ViewPlan));
                return aResult;
            }

            if (type == typeof(DraftingObject))
            {
                aResult.Add(typeof(FamilyInstance));
                return aResult;
            }

            if (type == typeof(oM.Adapters.Revit.Elements.Viewport))
            {
                aResult.Add(typeof(Autodesk.Revit.DB.Viewport));
                return aResult;
            }

            return null;
        }

        /***************************************************/
    }
}
