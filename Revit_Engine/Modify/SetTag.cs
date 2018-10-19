using System.Collections.Generic;

using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IBHoMObject SetTag(this IBHoMObject bHoMObject, string tag)
        {
            if (bHoMObject == null)
                return null;

            IBHoMObject aIBHoMObject = bHoMObject.GetShallowClone();

            if (aIBHoMObject.Tags == null)
                aIBHoMObject.Tags = new HashSet<string>();

            aIBHoMObject.Tags.Add(tag);

            return aIBHoMObject;
        }

        /***************************************************/
    }
}
