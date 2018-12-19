using Autodesk.Revit.DB;
using BH.oM.Base;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, Element element, bool convertUnits = true)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            foreach (Parameter aParameter in element.ParametersMap)
                aBHoMObject = SetCustomData(aBHoMObject, aParameter, convertUnits);

            return aBHoMObject;
        }

        /***************************************************/
        
        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, Element element, BuiltInParameter builtInParameter,  bool convertUnits = true)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            aBHoMObject = SetCustomData(aBHoMObject, element.get_Parameter(builtInParameter), convertUnits);
                

            return aBHoMObject;
        }

        /***************************************************/
        
        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, Parameter parameter, bool convertUnits = true)
        {
            if (bHoMObject == null || parameter == null)
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

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
        
        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, string customDataName, object value)
        {
            if (bHoMObject == null || string.IsNullOrEmpty(customDataName))
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            if (aBHoMObject.CustomData.ContainsKey(customDataName))
                aBHoMObject.CustomData[customDataName] = value;
            else
                aBHoMObject.CustomData.Add(customDataName, value);

            return aBHoMObject;
        }

        /***************************************************/
    }
}