using System.Collections.Generic;
using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Adds reference objects to existsing reference Dictionary. Method will create new dictionary if refObjects is null")]
        [Input("refObjects", "Existing reference objects")]
        [Input("bHoMObjects", "BHoM objects to be added")]
        [Output("RefObjects")]
        public static Dictionary<int, List<IBHoMObject>> AddRefObjects(this Dictionary<int, List<IBHoMObject>> refObjects, IEnumerable<IBHoMObject> bHoMObjects)
        {
            if (bHoMObjects == null && bHoMObjects == null)
                return null;

            Dictionary<int, List<IBHoMObject>> aResult = null;
            if (refObjects == null)
                aResult = new Dictionary<int, List<IBHoMObject>>();
            else
                aResult = new Dictionary<int, List<IBHoMObject>>(refObjects);

            foreach (BHoMObject aBHoMObject in bHoMObjects)
                aResult = aResult.AddRefObject(aBHoMObject);

            return aResult;
        }

        /***************************************************/
    }
}