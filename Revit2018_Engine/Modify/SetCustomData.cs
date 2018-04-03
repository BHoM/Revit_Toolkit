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
        /// Copy values to BHoMObject CustomData from Revit Element parameters
        /// </summary>
        /// <param name="bHoMObject">Destination BHoMObject</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="element">Source Revit Element</param>
        /// <returns name="BHoMObject">BHoMObject</returns>
        /// <search>
        /// Modify, BHoM, SetCustomData,  BHoMObject, Revit, Element, Set CustomData
        /// </search>
        public static BHoMObject SetCustomData(this BHoMObject bHoMObject, Element element, bool convertUnits = true)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            BHoMObject aBHoMObject = bHoMObject.GetShallowClone() as BHoMObject;

            foreach (Parameter aParameter in element.ParametersMap)
                aBHoMObject = SetCustomData(aBHoMObject, aParameter, convertUnits);

            return aBHoMObject;
        }

        /***************************************************/

        /// <summary>
        /// Copy value to BHoMObject CustomData from Revit Built-in Parameter
        /// </summary>
        /// <param name="bHoMObject">Destination BHoMObject</param>
        /// <param name="element">Source Revit Element</param>
        /// <param name="builtInParamater">Revit BuiltIn Paramater</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="BHoMObject">BHoMObject</returns>
        /// <search>
        /// Modify, BHoM, SetCustomData,  BHoMObject, Revit, Element, Set CustomData
        /// </search>
        public static BHoMObject SetCustomData(this BHoMObject bHoMObject, Element element, BuiltInParameter builtInParamater,  bool convertUnits = true)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            BHoMObject aBHoMObject = bHoMObject.GetShallowClone() as BHoMObject;

            aBHoMObject = SetCustomData(aBHoMObject, element.get_Parameter(builtInParamater), convertUnits);
                

            return aBHoMObject;
        }
        /***************************************************/

        /// <summary>
        /// Copy value to BHoMObject CustomData from Revit Parameter
        /// </summary>
        /// <param name="bHoMObject">Destination BHoMObject</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="parameter">Source Revit Element</param>
        /// <returns name="BHoMObject">BHoMObject</returns>
        /// <search>
        /// Modify, BHoM, SetCustomData,  BHoMObject, Revit, Element, Set CustomData, Parameter
        /// </search>
        public static BHoMObject SetCustomData(this BHoMObject bHoMObject, Parameter parameter, bool convertUnits = true)
        {
            if (bHoMObject == null || parameter == null)
                return bHoMObject;

            BHoMObject aBHoMObject = bHoMObject.GetShallowClone() as BHoMObject;

            object aValue = null;
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    aValue = parameter.AsDouble();
                    if (convertUnits)
                        aValue = Convert.ToSI((double)aValue, parameter.Definition.UnitType);
                    break;
                case StorageType.ElementId:
                    ElementId aElementId = parameter.AsElementId();
                    if (aElementId != null)
                        aValue = aElementId.IntegerValue;
                    break;
                case StorageType.Integer:
                    if (parameter.Definition.ParameterType == ParameterType.YesNo)
                        aValue = parameter.AsInteger() == 1;
                    else
                        aValue = parameter.AsInteger();
                    break;
                case StorageType.String:
                    aValue = parameter.AsString();
                    break;
                case StorageType.None:
                    aValue = parameter.AsValueString();
                    break;  
            }

            string aName = parameter.Definition.Name;
            if (aBHoMObject.CustomData.ContainsKey(aName))
                aBHoMObject.CustomData[aName] = aValue;
            else
                aBHoMObject.CustomData.Add(parameter.Definition.Name, aValue);

            return aBHoMObject;
        }

        /***************************************************/
    }
}
