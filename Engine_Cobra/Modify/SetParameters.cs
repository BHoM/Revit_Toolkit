using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static Element SetParameters(this Element element, BHoMObject bHoMObject, IEnumerable<BuiltInParameter> builtInParametersIgnore = null, bool convertUnits = true)
        {
            if (bHoMObject == null || element == null)
                return null;

            foreach (KeyValuePair<string, object> aKeyValuePair in bHoMObject.CustomData)
            {
                Parameter aParameter = element.LookupParameter(aKeyValuePair.Key);

                if (aParameter != null && !aParameter.IsReadOnly)
                {
                    if (builtInParametersIgnore != null && aParameter.Id.IntegerValue < 0 && builtInParametersIgnore.Contains((BuiltInParameter)aParameter.Id.IntegerValue))
                        continue;

                    SetParameter(aParameter, aKeyValuePair.Value, convertUnits);
                }
            }

            return element;
        }

        /***************************************************/
    }
}