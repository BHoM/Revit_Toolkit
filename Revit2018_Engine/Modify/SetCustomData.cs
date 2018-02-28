using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using BH.oM.Base;

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

        /// <summary>
        /// Copy values from BHoMObject CustomData to Revit Element parameters
        /// </summary>
        /// <param name="bHoMObject">Source BHoMObject</param>
        /// <param name="element">Destination Revit Element</param>
        /// <returns name="BHoMObject">BHoMObject</returns>
        /// <search>
        /// Modify, BHoM, SetCustomData,  BHoMObject, Revit, Element, Set CustomData
        /// </search>
        public static BHoMObject SetCustomData(this BHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return null;

            BHoMObject aBHoMObject = bHoMObject.GetShallowClone() as BHoMObject;

            foreach (Parameter aParameter in element.ParametersMap)
            {
                if (aBHoMObject.CustomData.ContainsKey(aParameter.Definition.Name))
                    continue;

                object aValue = null;
                switch (aParameter.StorageType)
                {
                    case StorageType.Double:
                        aValue = aParameter.AsDouble();
                        break;
                    case StorageType.ElementId:
                        ElementId aElementId = aParameter.AsElementId();
                        if (aElementId != null)
                            aValue = aElementId.IntegerValue;
                        break;
                    case StorageType.Integer:
                        if (aParameter.Definition.ParameterType == ParameterType.YesNo)
                            aValue = aParameter.AsInteger() == 1;
                        else
                            aValue = aParameter.AsInteger();
                        break;
                    case StorageType.String:
                        aValue = aParameter.AsString();
                        break;
                    case StorageType.None:
                        aValue = aParameter.AsValueString();
                        break;
                }

                aBHoMObject.CustomData.Add(aParameter.Definition.Name, aValue);
            }

            return aBHoMObject;
        }

        /***************************************************/
    }
}
