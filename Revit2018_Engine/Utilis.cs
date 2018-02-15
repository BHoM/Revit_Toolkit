using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Environmental.Properties;
using BH.oM.Environmental.Elements;


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

            public static List<ElementId> GetElementIdList(Document Document, IEnumerable<string> UniqueIds, bool RemoveNulls)
            {
                if (Document == null || UniqueIds == null)
                    return null;


                List<ElementId> aElementIdList = new List<ElementId>();
                foreach (string aUniqueId in UniqueIds)
                {
                    if(string.IsNullOrEmpty(aUniqueId))
                    {
                        Element aElement = Document.GetElement(aUniqueId);
                        if(aElement != null)
                        {
                            aElementIdList.Add(aElement.Id);
                            continue;
                        }
                    }

                    if (!RemoveNulls)
                        aElementIdList.Add(null);
                }

                return aElementIdList;
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

            public static void RemoveIdentifiers(BHoMObject BHoMObject)
            {
                if (BHoMObject == null)
                    return;

                BHoMObject.CustomData.Remove("ElementId");
                BHoMObject.CustomData.Remove("UniqueId");

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

            public static ElementId GetElemenId(BHoMObject BHoMObject)
            {
                if (BHoMObject == null)
                    return null;

                object aValue = null;
                if (BHoMObject.CustomData.TryGetValue("ElementId", out aValue))
                {
                    if (aValue is string)
                    {
                        int aInt = -1;
                        if (int.TryParse((string)aValue, out aInt))
                            return new ElementId(aInt);
                    }
                    else if(aValue is int)
                    {
                        return new ElementId((int)aValue);
                    }
                    else
                    {
                        return null;
                    } 
                }

                return null;
            }

            public static List<string> GetUniqueIdList(IEnumerable<BHoMObject> BHoMObjects, bool RemoveNulls = true)
            {
                if (BHoMObjects == null)
                    return null;

                List<string> aUniqueIdList = new List<string>();
                foreach (BHoMObject aBHoMObject in BHoMObjects)
                {
                    string aUniqueId = GetUniqueId(aBHoMObject);
                    if (string.IsNullOrEmpty(aUniqueId) && RemoveNulls)
                        continue;

                    aUniqueIdList.Add(aUniqueId);
                }

                return aUniqueIdList;
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

            public static Type GetType(Element Element)
            {
                if (Element is CeilingType)
                    return typeof(BuildingElementProperties);
                if (Element is WallType)
                    return typeof(BuildingElementProperties);
                if (Element is FloorType)
                    return typeof(BuildingElementProperties);
                if (Element is RoofType)
                    return typeof(BuildingElementProperties);
                if (Element.GetType().IsAssignableFrom(typeof(SpatialElement)))
                    return typeof(Space);
                if (Element is Wall)
                    return typeof(BuildingElement);
                if (Element is Ceiling)
                    return typeof(BuildingElement);
                if (Element.GetType().IsAssignableFrom(typeof(RoofBase)))
                    return typeof(BuildingElement);
                if (Element is Floor)
                    return typeof(BuildingElement);

                return null;
            }
        }
    }
}
