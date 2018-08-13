﻿using Autodesk.Revit.DB;
using BH.Engine.Revit;
using BH.Engine.Structure;
using BH.oM.Revit;
using BH.oM.Structural.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Wall ToRevitWall(this PanelPlanar panelPlanar, Document document, PushSettings pushSettings = null)
        {
            //TODO: if no CustomData, set the levels & base+top offsets manually?

            if (panelPlanar == null || document == null)
                return null;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            List<Curve> aCurves = panelPlanar.AllEdgeCurves().Select(c => c.ToRevit(pushSettings)).ToList();
            
            Level aLevel = null;

            aCustomDataValue = panelPlanar.ICustomData("Base Constraint");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(panelPlanar.Outline(), document);

            WallType aWallType = panelPlanar.Property.ToRevitWallType(document, pushSettings);

            if (aWallType == null)
            {
                List<WallType> aWallTypeList = new FilteredElementCollector(document).OfClass(typeof(WallType)).OfCategory(BuiltInCategory.OST_Walls).Cast<WallType>().ToList();

                aCustomDataValue = panelPlanar.ICustomData("Type");
                if (aCustomDataValue != null && aCustomDataValue is int)
                {
                    ElementId aElementId = new ElementId((int)aCustomDataValue);
                    aWallType = aWallTypeList.Find(x => x.Id == aElementId);
                }

                if (aWallType == null)
                    aWallTypeList.Find(x => x.Name == panelPlanar.Name);

                if (aWallType == null)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Wall type has not been found for given BHoM panel property. BHoM Guid: {0}", panelPlanar.BHoM_Guid));
                    return null;
                }
            }

            Wall aWall = Wall.Create(document, aCurves, aWallType.Id, aLevel.Id, false);

            aWall.CheckIfNullPush(panelPlanar);

            if (aWall != null)
            {
                if (pushSettings.CopyCustomData)
                {
                    BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                    {
                        BuiltInParameter.ELEM_FAMILY_PARAM,
                        BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                        BuiltInParameter.ALL_MODEL_IMAGE,
                        BuiltInParameter.WALL_KEY_REF_PARAM,
                        BuiltInParameter.ELEM_TYPE_PARAM,
                        //BuiltInParameter.WALL_BASE_CONSTRAINT,
                        //BuiltInParameter.WALL_BASE_OFFSET,
                        //BuiltInParameter.WALL_HEIGHT_TYPE,
                        //BuiltInParameter.WALL_TOP_OFFSET
                    };
                    Modify.SetParameters(aWall, panelPlanar, paramsToIgnore, pushSettings.ConvertUnits);
                }
            }

            return aWall;
        }
    }
}
