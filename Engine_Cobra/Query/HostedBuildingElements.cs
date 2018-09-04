using Autodesk.Revit.DB;
using BH.Engine.Environment;
using BH.oM.Environment.Elements;
using System.Collections.Generic;
using BH.oM.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public List<BuildingElement> HostedBuildingElements(HostObject hostObject, Face face, PullSettings pullSettings = null)
        {
            if (hostObject == null)
                return null;

            IList<ElementId> aElementIdList = hostObject.FindInserts(false, false, false, false);
            if (aElementIdList == null || aElementIdList.Count < 1)
                return null;

            pullSettings.DefaultIfNull();

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            foreach (ElementId aElementId in aElementIdList)
            {
                Element aElement_Hosted = hostObject.Document.GetElement(aElementId);
                if ((BuiltInCategory)aElement_Hosted.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Windows || (BuiltInCategory)aElement_Hosted.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Doors)
                {
                    IntersectionResult aIntersectionResult = null;

                    BoundingBoxXYZ aBoundingBoxXYZ = aElement_Hosted.get_BoundingBox(null);

                    aIntersectionResult = face.Project(aBoundingBoxXYZ.Max);
                    if (aIntersectionResult == null)
                        continue;

                    XYZ aXYZ_Max = aIntersectionResult.XYZPoint;
                    UV aUV_Max = aIntersectionResult.UVPoint;

                    aIntersectionResult = face.Project(aBoundingBoxXYZ.Min);
                    if (aIntersectionResult == null)
                        continue;

                    XYZ aXYZ_Min = aIntersectionResult.XYZPoint;
                    UV aUV_Min = aIntersectionResult.UVPoint;

                    double aU = aUV_Max.U - aUV_Min.U;
                    double aV = aUV_Max.V - aUV_Min.V;

                    XYZ aXYZ_V = face.Evaluate(new UV(aUV_Max.U, aUV_Max.V - aV));
                    if (aXYZ_V == null)
                        continue;

                    XYZ aXYZ_U = face.Evaluate(new UV(aUV_Max.U - aU, aUV_Max.V));
                    if (aXYZ_U == null)
                        continue;

                    List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
                    aPointList.Add(aXYZ_Max.ToBHoM(pullSettings));
                    aPointList.Add(aXYZ_U.ToBHoM(pullSettings));
                    aPointList.Add(aXYZ_Min.ToBHoM(pullSettings));
                    aPointList.Add(aXYZ_V.ToBHoM(pullSettings));
                    aPointList.Add(aXYZ_Max.ToBHoM(pullSettings));

                    BuildingElementPanel aBuildingElementPanel = Create.BuildingElementPanel(new oM.Geometry.Polyline[] { BH.Engine.Geometry.Create.Polyline(aPointList) });

                    BuildingElement aBuildingElement = Convert.ToBHoMBuildingElement(aElement_Hosted, aBuildingElementPanel, pullSettings);
                    if (aBuildingElement != null)
                        aBuildingElmementList.Add(aBuildingElement);

                }
            }
            return aBuildingElmementList;
        }

        /***************************************************/
    }
}