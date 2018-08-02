using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Geometry;
using BH.Engine.Environment;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Element ToRevit(this BuildingElement buildingElement, Document document, bool copyCustomData = true, bool convertUnits = true)
        {
            if (buildingElement == null || buildingElement.BuildingElementProperties == null || document == null)
                return null;

            //Get Level
            Level aLevel = null;
            if (buildingElement.Level != null)
            {
                List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
                if (aLevelList != null && aLevelList.Count > 0)
                    aLevel = aLevelList.Find(x => x.Name == buildingElement.Level.Name);
            }

            //Get ElementType
            ElementType aElementType = null;
            Type aType = Query.RevitType(buildingElement.BuildingElementProperties.BuildingElementType);
            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(aType).Cast<ElementType>().ToList();
            if (aElementTypeList != null && aElementTypeList.Count > 0)
            {
                aElementType = aElementTypeList.Find(x => x.Name == buildingElement.BuildingElementProperties.Name);
                if (aElementType == null)
                    aElementType = aElementTypeList.First();
            }

            Element aElement = null;

            BuiltInParameter[] aBuiltInParameters = null;
            switch (buildingElement.BuildingElementProperties.BuildingElementType)
            {
                case BuildingElementType.Ceiling:
                    //TODO: Create Ceiling from BuildingElement
                    break;
                case BuildingElementType.Floor:
                    if (buildingElement.BuildingElementGeometry is BuildingElementPanel)
                    {
                        BuildingElementPanel aBuildingElementPanel = buildingElement.BuildingElementGeometry as BuildingElementPanel;

                        if (aElementType != null)
                            aElement = document.Create.NewFloor(aBuildingElementPanel.PolyCurve.ToRevit(convertUnits), aElementType as FloorType, aLevel, false);
                        else
                            aElement = document.Create.NewFloor(aBuildingElementPanel.PolyCurve.ToRevit(convertUnits), false);
                    }
                    aBuiltInParameters = new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM };
                    break;
                case BuildingElementType.Roof:
                    if (buildingElement.BuildingElementGeometry is BuildingElementPanel)
                    {
                        //TODO: Check Roof creation

                        BuildingElementPanel aBuildingElementPanel = buildingElement.BuildingElementGeometry as BuildingElementPanel;
                        ModelCurveArray aModelCurveArray = new ModelCurveArray();
                        if (aElementType != null)
                            aElement = document.Create.NewFootPrintRoof(aBuildingElementPanel.PolyCurve.ToRevit(convertUnits), aLevel, aElementType as RoofType, out aModelCurveArray);
                    }
                    break;
                case BuildingElementType.Wall:
                    ICurve aICurve = buildingElement.BuildingElementGeometry.Bottom();
                    if (aICurve == null)
                        return null;
                    aElement = Wall.Create(document, ToRevit(aICurve, convertUnits), aLevel.Id, false);
                    if (aElementType != null)
                        aElement.ChangeTypeId(aElementType.Id);

                    aBuiltInParameters = new BuiltInParameter[] { BuiltInParameter.WALL_BASE_CONSTRAINT };
                    break;
            }

            if (copyCustomData)
                Modify.SetParameters(aElement, buildingElement, aBuiltInParameters, convertUnits);

            return aElement;
        }

        /***************************************************/
    }
}

