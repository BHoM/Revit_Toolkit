using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<IBHoMObject>> AddBHoMObjects(this Dictionary<ElementId, List<IBHoMObject>> objects, IEnumerable<IBHoMObject> bHoMObjects)
        {
            if (bHoMObjects == null && bHoMObjects == null)
                return null;

            Dictionary<ElementId, List<IBHoMObject>> aResult = null;
            if (objects == null)
                aResult = new Dictionary<ElementId, List<IBHoMObject>>();
            else
                aResult = new Dictionary<ElementId, List<IBHoMObject>>(objects);

            foreach (BHoMObject aBHoMObject in bHoMObjects)
                aResult = aResult.AddBHoMObject(aBHoMObject);

            return aResult;
        }

        /***************************************************/
    }
}

