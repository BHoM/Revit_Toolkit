using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Environment;
using BH.oM.Environment.Elements;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static Element ToRevitElement(this BuildingElement buildingElement, Document document, PushSettings pushSettings = null)
        {
            if (buildingElement == null || buildingElement.BuildingElementProperties == null || document == null)
                return null;

            ElementType aElementType = null;

            if(buildingElement.BuildingElementProperties != null)
                aElementType = buildingElement.BuildingElementProperties.ToRevitElementType(document, pushSettings);
            
            if(aElementType == null)
            {
                string aFamilyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(buildingElement);
                if(!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList().FindAll(x => x.Name == aFamilyTypeName && x.Category != null);
                    aElementType = aElementTypeList.First();
                }
            }

            if (aElementType == null)
            {
                string aFamilyTypeName = buildingElement.Name;
                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList().FindAll(x => x.Name == aFamilyTypeName && x.Category != null);
                    aElementType = aElementTypeList.First();
                }
            }

            if (aElementType == null)
                return null;

            BuildingElementType? aBuildingElementType = null;
            if (buildingElement.BuildingElementProperties != null)
                aBuildingElementType = buildingElement.BuildingElementProperties.BuildingElementType;

            if (aBuildingElementType == null || !aBuildingElementType.HasValue || aBuildingElementType == BuildingElementType.Undefined)
                aBuildingElementType = Query.BuildingElementType(aElementType.Category);

            if (aBuildingElementType == null || !aBuildingElementType.HasValue)
                return null;

            Element aElement = null;

            BuiltInParameter[] aBuiltInParameters = null;
            switch (aBuildingElementType.Value)
            {
                case BuildingElementType.Ceiling:
                    //TODO: Create Ceiling from BuildingElement
                    break;
                case BuildingElementType.Floor:
                    if (aElementType != null)
                    {
                        Level aLevel = document.Level(buildingElement.MinimumLevel(), true);
                        aElement = document.Create.NewFloor((buildingElement.PanelCurve as PolyCurve).ToRevitCurveArray(pushSettings), aElementType as FloorType, aLevel, false);
                    }
                    else
                    {
                        aElement = document.Create.NewFloor((buildingElement.PanelCurve as PolyCurve).ToRevitCurveArray(pushSettings), false);
                    }  
                    aBuiltInParameters = new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM };
                    break;
                case BuildingElementType.Roof:
                    ModelCurveArray aModelCurveArray = new ModelCurveArray();
                    if (aElementType != null)
                    {
                        Level aLevel = document.Level(buildingElement.MinimumLevel(), true);
                        double aElevation = aLevel.Elevation;
                        if (pushSettings.ConvertUnits)
                            aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

                        oM.Geometry.Plane aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aElevation), BH.Engine.Geometry.Create.Vector(0, 0, 1));
                        ICurve aCurve = BH.Engine.Geometry.Modify.Project(buildingElement.PanelCurve as dynamic, aPlane) as ICurve;
                        FootPrintRoof aFootPrintRoof = document.Create.NewFootPrintRoof((aCurve as PolyCurve).ToRevitCurveArray(pushSettings), aLevel, aElementType as RoofType, out aModelCurveArray);
                        if(aFootPrintRoof != null)
                        {
                            List<ICurve> aCurveList = (buildingElement.PanelCurve as PolyCurve).Curves;
                            if (aCurveList != null && aCurveList.Count > 0)
                            {
                                SlabShapeEditor aSlabShapeEditor = aFootPrintRoof.SlabShapeEditor;
                                aSlabShapeEditor.ResetSlabShape();

                                List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
                                foreach(ICurve aCurve_Temp in aCurveList)
                                    aSlabShapeEditor.DrawPoint(BH.Engine.Geometry.Query.IStartPoint(aCurve_Temp).ToRevit(pushSettings));
                            }

                            aElement = aFootPrintRoof;
                        }
                    }
                        
                    break;
                case BuildingElementType.Wall:
                    aElement = Wall.Create(document, Convert.ToRevitCurveIList(buildingElement.Curve(), pushSettings), false); //(document, ToRevitCurve(aICurve, pushSettings), aLevel.Id, false);
                    if (aElementType != null)
                        aElement.ChangeTypeId(aElementType.Id);

                    aBuiltInParameters = new BuiltInParameter[] { BuiltInParameter.WALL_BASE_CONSTRAINT };
                    break;
                case BuildingElementType.Door:
                    //TODO: Create Door from BuildingElement
                    break;
                case BuildingElementType.Window:
                    //TODO: Create Window from BuildingElement
                    break;
            }

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, buildingElement, aBuiltInParameters, pushSettings.ConvertUnits);

            return aElement;
        }

        /***************************************************/
    }
}