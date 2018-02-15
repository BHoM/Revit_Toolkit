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
    /// <summary>
    /// BHoM Revit Engine Utilities
    /// </summary>
    public static class Utilis
    {
        /// <summary>
        /// Utilities for Revit objects
        /// </summary>
        public static class Revit
        {
            /***************************************************/
            /**** Public Methods                            ****/
            /***************************************************/

            /// <summary>
            /// Get Revit class type from BHoM BuildingElementType.
            /// </summary>
            /// <param name="BuildingElementType">BHoM BuildingElementType</param>
            /// <returns name="Type">Revit class Type</returns>
            /// <search>
            /// Utilis, GetType, Revit, Get Type, BuildingElementType
            /// </search>
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

            /// <summary>
            /// Copy Revit ELement parameters to BHoMObject CustomData. Only parameters with unique names will be copied. If Revit Element has more than two parameters with the same name first parameters returned by Element.LookupParameter function will be returned. Built-In Parameters from BuiltInParameters_Ignore collection wil be ignored
            /// </summary>
            /// <param name="BHoMObject">Destination BHoMObject</param>
            /// <param name="Element">Source Revit Element</param>
            /// <param name="BuiltInParameters_Ignore">Collection of Built-In Revit Parameters to be excluded from copy</param>
            /// <search>
            /// Utilis, CopyCustomData, Revit, Copy Custom Data, BHoMObject, Element
            /// </search>
            public static void CopyCustomData(BHoMObject BHoMObject, Element Element, IEnumerable<BuiltInParameter> BuiltInParameters_Ignore = null)
            {
                if (BHoMObject == null || Element == null)
                    return;

                foreach (KeyValuePair<string, object> aKeyValuePair in BHoMObject.CustomData)
                {
                    Parameter aParameter = Element.LookupParameter(aKeyValuePair.Key);

                    if (BuiltInParameters_Ignore != null && aParameter.Id.IntegerValue < 0 && BuiltInParameters_Ignore.Contains((BuiltInParameter)aParameter.Id.IntegerValue))
                        continue;

                    if (aParameter != null && !aParameter.IsReadOnly)
                        CopyParameter(aParameter, aKeyValuePair.Value);
                }
            }

            /// <summary>
            /// Copy Value to Revit Parameter. If parameter has units then value needs to be in Revit internal units: use UnitUtils.ConvertToInternalUnits (Autodesk.Revit.DB) to convert units
            /// </summary>
            /// <param name="Parameter">Revit Parameter</param>
            /// <param name="Value">Value for parameter to be set. StorageType: 1. Double - Value type of double, int, bool, string 2. ElementId - Value type of int, string, 3. Integer - Value type of double, int, bool, string 4. String - value type of string, object</param>
            /// <search>
            /// Utilis, CopyParameter, Revit, Copy Parameter, Parameter
            /// </search>
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
                        else if(Value is string)
                        {
                            double aDouble = double.NaN;
                            if (double.TryParse((string)Value, out aDouble))
                                Parameter.Set(aDouble);
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
                        else if (Value is string)
                        {
                            int aInt = 0;
                            if (int.TryParse((string)Value, out aInt))
                                Parameter.Set(aInt);
                        }
                        break;

                    case StorageType.String:
                        if(Value == null)
                        {
                            string aString = null;
                            Parameter.Set(aString);
                        }
                        else if (Value is string)
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

            /// <summary>
            /// Gets ElementId list from UniqueIds collection
            /// </summary>
            /// <param name="Document">Revit Document</param>
            /// <param name="UniqueIds">Unique Ids collection</param>
            /// <param name="RemoveNulls">Remove nulls or empty values from result</param>
            /// <returns name="ElementIds">ElementId List</returns>
            /// <search>
            /// Utilis, GetElementIdList, Revit, Get ElementId List, Parameter
            /// </search>
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

            /***************************************************/
        }

        /// <summary>
        /// Utilities for BHoM objects
        /// </summary>
        public static class BHoM
        {
            /***************************************************/
            /**** Public Methods                            ****/
            /***************************************************/

            /// <summary>
            /// Gets BuildingElementType from Revit BuiltInCategory. If no match then null will be returned.
            /// </summary>
            /// <param name="BuiltInCategory">Revit BuiltInCategory</param>
            /// <returns name="BuildingElementType">BHoM BuildingElementType</returns>
            /// <search>
            /// Utilis, BHoM, GetBuildingElementType, Get BuildingElementType, BuiltInCategory, Revit
            /// </search>
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

            /// <summary>
            /// Copies Revit Identifiers to BHoMObject CustomData. Key: "ElementId" Value: Element.Id.IntegerValue (storage type int), Key: "UniqueId" Value: Element.UniqueId (storage type string)
            /// </summary>
            /// <param name="BHoMObject">BHoMObject</param>
            /// <param name="Element">Revit Element</param>
            /// <search>
            /// Utilis, BHoM, CopyIdentifiers, Revit, Copy Identifiers, BHoMObject
            /// </search>
            public static void CopyIdentifiers(BHoMObject BHoMObject, Element Element)
            {
                if (BHoMObject == null || Element == null)
                    return;

                BHoMObject.CustomData.Add("ElementId", Element.Id.IntegerValue);
                BHoMObject.CustomData.Add("UniqueId", Element.UniqueId);
            }

            /// <summary>
            /// Removes Revit Identifiers from BHoMObject CustomData. Key: "ElementId", Key: "UniqueId"
            /// </summary>
            /// <param name="BHoMObject">BHoMObject</param>
            /// <search>
            /// Utilis, BHoM, RemoveIdentifiers, Remove Identifiers, BHoMObject
            /// </search>
            public static void RemoveIdentifiers(BHoMObject BHoMObject)
            {
                if (BHoMObject == null)
                    return;

                BHoMObject.CustomData.Remove("ElementId");
                BHoMObject.CustomData.Remove("UniqueId");

            }

            /// <summary>
            /// Reads Revit UniqueId from BHoMObject CustomData. Key: "UniqueId"
            /// </summary>
            /// <param name="BHoMObject">BHoMObject</param>
            /// <returns name="UniqueId">Revit UniqueId</returns>
            /// <search>
            /// Utilis, BHoM, GetUniqueId, Get UniqueId, BHoMObject
            /// </search>
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

            /// <summary>
            /// Reads Revit ElementId from BHoMObject CustomData. Key: "ElementId"
            /// </summary>
            /// <param name="BHoMObject">BHoMObject</param>
            /// <returns name="ElementId">Revit ElementId</returns>
            /// <search>
            /// Utilis, BHoM, GetElemenId, Get ElemenId, BHoMObject
            /// </search>
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

            /// <summary>
            /// Reads Revit UniqueIds from BHoMObjects CustomData. Key: "UniqueId"
            /// </summary>
            /// <param name="BHoMObjects">BHoMObject collection</param>
            /// <param name="RemoveNulls">Removes nulls from result list</param>
            /// <returns name="UniqueIds">UniqueId List</returns>
            /// <search>
            /// Utilis, BHoM, GetUniqueIdList, Get UniqueId List, BHoMObject
            /// </search>
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

            /// <summary>
            /// Copy values from BHoMObject CustomData to Revit Element parameters
            /// </summary>
            /// <param name="BHoMObject">Source BHoMObject</param>
            /// <param name="Element">Destination Revit Element</param>
            /// <search>
            /// Utilis, BHoM, CopyCustomData,  BHoMObject, Revit, Element, Copy CustomData
            /// </search>
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

            /// <summary>
            /// Gets BHoM class type for given Element
            /// </summary>
            /// <param name="Element">Revit Element</param>
            /// <returns name="Type">BHoM class Type</returns>
            /// <search>
            /// Utilis, BHoM, GetType,  BHoMObject, Revit, Element, Get Type
            /// </search>
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

            /***************************************************/
        }
    }
}
