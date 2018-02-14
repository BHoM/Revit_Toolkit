using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Elements;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    public static class Utilis
    {
        public static class Revit
        {
            public static Type GetType(BuildingElementType BuildingElementType)
            {
                switch (BuildingElementType)
                {
                    case BuildingElementType.Ceiling:
                        return typeof(CeilingType);
                    case BuildingElementType.Floor:
                        return typeof(FloorType);
                    case BuildingElementType.Roof:
                        return typeof(RoofType);
                    case BuildingElementType.Wall:
                        return typeof(WallType);
                }

                return null;
            }

            public static void CopyCustomData(BHoMObject BHoMObject, Element Element)
            {
                if (BHoMObject == null || Element == null)
                    return;

                foreach (KeyValuePair<string, object> aKeyValuePair in BHoMObject.CustomData)
                {
                    Parameter aParameter = Element.LookupParameter(aKeyValuePair.Key);
                    if(aParameter != null && !aParameter.IsReadOnly)
                        CopyParameter(aParameter, aKeyValuePair.Value);
                }
            }

            public static void CopyParameter(Parameter Parameter, object Value)
            {
                if (Parameter == null)
                    return;

                switch (Parameter.StorageType)
                {
                    case StorageType.Double:
                        if(Value is double || Value is int)
                        {
                            Parameter.Set((double)Value);
                        }
                        else if(Value is bool)
                        {
                            if ((bool)Value)
                                Parameter.Set(1.0);
                            else
                                Parameter.Set(0.0);
                        }
                        break;
                    case StorageType.ElementId:
                        if (Value is int)
                        {
                            Parameter.Set(new ElementId((int)Value));
                        }
                        else if(Value is string)
                        {
                            int aInt ;
                            if(int.TryParse((string)Value, out aInt))
                            {
                                Parameter.Set(new ElementId(aInt));
                            }

                        }
                        break;
                    case StorageType.Integer:
                        if (Value is double || Value is int)
                        {
                            Parameter.Set((int)Value);
                        }
                        else if (Value is bool)
                        {
                            if ((bool)Value)
                                Parameter.Set(1);
                            else
                                Parameter.Set(0);
                        }
                        break;

                    case StorageType.String:
                        if(Value == null)
                        {
                            string aString = null;
                            Parameter.Set(aString);
                        }
                        if (Value is string)
                        {
                            Parameter.Set((string)Value);
                        }
                        else
                        {
                            Parameter.Set(Value.ToString());
                        }
                        break;

                }
            }
        }

        public static class BHoM
        {
            public static BuildingElementType? GetBuildingElementType(BuiltInCategory BuiltInCategory)
            {
                switch(BuiltInCategory)
                {
                    case BuiltInCategory.OST_Ceilings:
                        return BuildingElementType.Ceiling;
                    case BuiltInCategory.OST_Floors:
                        return BuildingElementType.Floor;
                    case BuiltInCategory.OST_Roofs:
                        return BuildingElementType.Roof;
                    case BuiltInCategory.OST_Walls:
                        return BuildingElementType.Wall;
                }

                return null;
            }

            public static void CopyIdentifiers(BHoMObject BHoMObject, Element Element)
            {
                if (BHoMObject == null || Element == null)
                    return;

                BHoMObject.CustomData.Add("ElementId", Element.Id.IntegerValue);
                BHoMObject.CustomData.Add("UniqueId", Element.UniqueId);
            }

            public static string GetUniqueId(BHoMObject BHoMObject)
            {
                if (BHoMObject == null)
                    return null;

                object aValue = null;
                if(BHoMObject.CustomData.TryGetValue("UniqueId", out aValue))
                {
                    if (aValue is string)
                        return (string)aValue;
                    else
                        return null;
                }

                return null;
            }

            public static void CopyCustomData(BHoMObject BHoMObject, Element Element)
            {
                if (BHoMObject == null || Element == null)
                    return;

                foreach (Parameter aParameter in Element.ParametersMap)
                {
                    if (BHoMObject.CustomData.ContainsKey(aParameter.Definition.Name))
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

                    BHoMObject.CustomData.Add(aParameter.Definition.Name, aValue);
                }
            }
        }
    }
}
