using System.Collections.Generic;
using BH.oM.Adapters.Revit;
using System.IO;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Moves GenericObject by given vector.")]
        [Input("genericObject", "GenericObject to be moved")]
        [Input("vector", "Vector")]
        [Output("GenericObject")]
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
