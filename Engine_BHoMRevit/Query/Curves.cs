using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public List<oM.Geometry.ICurve> Curves(this GeometryElement geometryElement, Transform transform = null, PullSettings pullSettings = null)
        {
            if (geometryElement == null)
                return null;

            List<oM.Geometry.ICurve> aResult = new List<oM.Geometry.ICurve>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                if (aGeometryObject is Solid)
                {
                    Solid aSolid = (Solid)aGeometryObject;
                    EdgeArray aEdgeArray = aSolid.Edges;
                    if (aEdgeArray == null)
                        continue;

                    List<oM.Geometry.ICurve> aCurveList = aEdgeArray.ToBHoM(pullSettings);
                    if (aCurveList != null && aCurveList.Count != 0)
                        aResult.AddRange(aCurveList);                        
                }
                else if (aGeometryObject is GeometryInstance)
                {
                    GeometryInstance aGeometryInstance = (GeometryInstance)aGeometryObject;

                    Transform aTransform = aGeometryInstance.Transform;
                    if (transform != null)
                        aTransform = aTransform.Multiply(transform.Inverse);


                    GeometryElement aGeometryElement = aGeometryInstance.GetInstanceGeometry(aTransform);
                    if (aGeometryElement == null)
                        continue;

                    List<oM.Geometry.ICurve> aCurveList = Curves(aGeometryElement);
                    if (aCurveList != null && aCurveList.Count != 0)
                        aResult.AddRange(aCurveList);
                }
            }
            return aResult;
        }

        /***************************************************/

        static public List<oM.Geometry.ICurve> Curves(this Element element, Options options, PullSettings pullSettings = null)
        {
            GeometryElement aGeometryElement = element.get_Geometry(options);

            Transform aTransform = null;
            if (element is FamilyInstance)
                aTransform = ((FamilyInstance)element).GetTotalTransform();

            return Curves(aGeometryElement, aTransform, pullSettings);
        }

        /***************************************************/
    }
}
