using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.Engine.Adapters.Revit;
using BH.Engine.Structure;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;

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

            Wall aWall = pushSettings.FindRefObject<Wall>(document, panelPlanar.BHoM_Guid);
            if (aWall != null)
                return aWall;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            List<Curve> aCurves = panelPlanar.ExternalEdgeCurves().Select(c => c.ToRevitCurve(pushSettings)).ToList();
            if (panelPlanar.Openings.Count != 0) panelPlanar.OpeningInPanelWarning();

            Level aLevel = null;

            aCustomDataValue = panelPlanar.CustomDataValue("Base Constraint");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(panelPlanar.Outline(), document, pushSettings.ConvertUnits);

            WallType aWallType = panelPlanar.Property.ToRevitWallType(document, pushSettings);

            if (aWallType == null)
            {
                aWallType = Query.ElementType(panelPlanar, document, BuiltInCategory.OST_Walls) as WallType;

                if (aWallType == null)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Wall type has not been found for given BHoM panel property. BHoM Guid: {0}", panelPlanar.BHoM_Guid));
                    return null;
                }
            }

            aWall = Wall.Create(document, aCurves, aWallType.Id, aLevel.Id, true);

            aWall.CheckIfNullPush(panelPlanar);
            if (aWall == null)
                return null;

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

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(panelPlanar, aWall);

            return aWall;
        }
    }
}
