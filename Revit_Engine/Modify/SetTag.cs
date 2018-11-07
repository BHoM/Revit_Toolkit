using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Tag for BHoMObject.")]
        [Input("bHoMObject", "BHoMObject")]
        [Input("tag", "tag to be set")]
        [Output("IBHoMObject")]
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
