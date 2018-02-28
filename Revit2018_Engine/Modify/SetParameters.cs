using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Mutable method which copy values from BHoMObject CustomData to Revit Element parameters. 
        /// </summary>
        /// <param name="bHoMObject">Source BHoMObject</param>
        /// <param name="element">Destination Revit Element</param>
        /// <param name="builtInParametersIgnore">Collection of Built-In Revit Parameters to be excluded from copy</param>
        /// <returns name="Element">Revit Element</returns>
        /// <search>
        /// Modify, BHoM, SetParameters,  BHoMObject, Revit, Element, Set Parameters
        /// </search>
        public static Element SetParameters(this Element element, BHoMObject bHoMObject, IEnumerable<BuiltInParameter> builtInParametersIgnore = null)
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

                    SetParameter(aParameter, aKeyValuePair.Value);
                }
            }

            return element;
        }

        /***************************************************/
    }
}
