using System.Collections.Generic;
using BH.oM.Adapters.Revit;
using System.IO;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static GenericObject Move(this GenericObject genericObject, Vector vector)
        {
            if (genericObject == null)
                return null;

            GenericObject aGenericObject = genericObject.GetShallowClone() as GenericObject;

            aGenericObject.Location = Geometry.Modify.Translate(aGenericObject.Location as dynamic, vector);
            
            return aGenericObject;
        }

        /***************************************************/
    }
}
