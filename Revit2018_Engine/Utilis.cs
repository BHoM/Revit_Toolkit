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
        /// RevitAdapter Id - CustomData Adapter object identification parameter name
        /// </summary>
        public const string AdapterId = "Revit_id";

        /// <summary>
        /// ElementId - CustomData Revit idetification parameter name
        /// </summary>
        public const string ElementId = "Revit_elementId";

        /// <summary>
        /// Utilities for Revit objects
        /// </summary>
        public static class Revit
        {
            /***************************************************/
            /**** Private Properties                        ****/
            /***************************************************/

            /// <summary>
            /// Machine epsilon - upper bound on the relative error due to rounding in floating point arithmetic.
            /// </summary>
            private const double pEpsilon = 1.0e-9;

            /***************************************************/
            /**** Public Methods                            ****/
            /***************************************************/

            /// <summary>
            /// Get Revit class type from BHoM BuildingElementType.
            /// </summary>
            /// <param name="buildingElementType">BHoM BuildingElementType</param>
            /// <returns name="Type">Revit class Type</returns>
            /// <search>
            /// Utilis, GetType, Revit, Get Type, BuildingElementType
            /// </search>
            public static System.Type GetType(BuildingElementType buildingElementType)
            {
                switch (buildingElementType)
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
            /// Get Revit class types from BHoM class type.
            /// </summary>
            /// <param name="Type">BHoM class Type</param>
            /// <returns name="Types">Revit class Types</returns>
            /// <search>
            /// Utilis, GetTypes, Revit, Get Types, Type, BHoM Type
            /// </search>
            public static IEnumerable<System.Type> GetTypes(System.Type Type)
            {
                if (Type == null)
                    return null;

                if (!Utilis.Type.IsAssignableFromByFullName(typeof(BHoMObject), Type))
                    return null;

                List<System.Type> aResult = new List<System.Type>();
                if(Type == typeof(BuildingElement))
                {
                    aResult.Add(typeof(Floor));
                    aResult.Add(typeof(Wall));
                    aResult.Add(typeof(Ceiling));
                    aResult.Add(typeof(RoofBase));
                    return aResult;
                }

                return null;
            }

            /// <summary>
            /// Copy Revit ELement parameters to BHoMObject CustomData. Only parameters with unique names will be copied. If Revit Element has more than two parameters with the same name first parameters returned by Element.LookupParameter function will be returned. Built-In Parameters from BuiltInParameters_Ignore collection wil be ignored
            /// </summary>
            /// <param name="bHoMObject">Destination BHoMObject</param>
            /// <param name="element">Source Revit Element</param>
            /// <param name="builtInParametersIgnore">Collection of Built-In Revit Parameters to be excluded from copy</param>
            /// <search>
            /// Utilis, CopyCustomData, Revit, Copy Custom Data, BHoMObject, Element
            /// </search>
            public static void CopyCustomData(BHoMObject bHoMObject, Element element, IEnumerable<BuiltInParameter> builtInParametersIgnore = null)
            {
                if (bHoMObject == null || element == null)
                    return;

                foreach (KeyValuePair<string, object> aKeyValuePair in bHoMObject.CustomData)
                {
                    Parameter aParameter = element.LookupParameter(aKeyValuePair.Key);

                    if (aParameter != null && !aParameter.IsReadOnly)
                    {
                        if (builtInParametersIgnore != null && aParameter.Id.IntegerValue < 0 && builtInParametersIgnore.Contains((BuiltInParameter)aParameter.Id.IntegerValue))
                            continue;

                        CopyParameter(aParameter, aKeyValuePair.Value);
                    }
                        
                }
            }

            /// <summary>
            /// Copy Value to Revit Parameter. If parameter has units then value needs to be in Revit internal units: use UnitUtils.ConvertToInternalUnits (Autodesk.Revit.DB) to convert units
            /// </summary>
            /// <param name="parameter">Revit Parameter</param>
            /// <param name="value">Value for parameter to be set. StorageType: 1. Double - Value type of double, int, bool, string 2. ElementId - Value type of int, string, 3. Integer - Value type of double, int, bool, string 4. String - value type of string, object</param>
            /// <search>
            /// Utilis, CopyParameter, Revit, Copy Parameter, Parameter
            /// </search>
            public static void CopyParameter(Parameter parameter, object value)
            {
                if (parameter == null)
                    return;

                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        if(value is double || value is int)
                        {
                            parameter.Set((double)value);
                        }
                        else if(value is bool)
                        {
                            if ((bool)value)
                                parameter.Set(1.0);
                            else
                                parameter.Set(0.0);
                        }
                        else if(value is string)
                        {
                            double aDouble = double.NaN;
                            if (double.TryParse((string)value, out aDouble))
                                parameter.Set(aDouble);
                        }
                        break;
                    case StorageType.ElementId:
                        if (value is int)
                        {
                            parameter.Set(new ElementId((int)value));
                        }
                        else if(value is string)
                        {
                            int aInt ;
                            if(int.TryParse((string)value, out aInt))
                            {
                                parameter.Set(new ElementId(aInt));
                            }

                        }
                        break;
                    case StorageType.Integer:
                        if (value is double || value is int)
                        {
                            parameter.Set((int)value);
                        }
                        else if (value is bool)
                        {
                            if ((bool)value)
                                parameter.Set(1);
                            else
                                parameter.Set(0);
                        }
                        else if (value is string)
                        {
                            int aInt = 0;
                            if (int.TryParse((string)value, out aInt))
                                parameter.Set(aInt);
                        }
                        break;

                    case StorageType.String:
                        if(value == null)
                        {
                            string aString = null;
                            parameter.Set(aString);
                        }
                        else if (value is string)
                        {
                            parameter.Set((string)value);
                        }
                        else
                        {
                            parameter.Set(value.ToString());
                        }
                        break;

                }
            }

            /// <summary>
            /// Gets ElementId list from UniqueIds collection
            /// </summary>
            /// <param name="document">Revit Document</param>
            /// <param name="uniqueIds">Unique Ids collection</param>
            /// <param name="removeNulls">Remove nulls or empty values from result</param>
            /// <returns name="ElementIds">ElementId List</returns>
            /// <search>
            /// Utilis, GetElementIdList, Revit, Get ElementId List, Parameter
            /// </search>
            public static List<ElementId> GetElementIdList(Document document, IEnumerable<string> uniqueIds, bool removeNulls)
            {
                if (document == null || uniqueIds == null)
                    return null;


                List<ElementId> aElementIdList = new List<ElementId>();
                foreach (string aUniqueId in uniqueIds)
                {
                    if(string.IsNullOrEmpty(aUniqueId))
                    {
                        Element aElement = document.GetElement(aUniqueId);
                        if(aElement != null)
                        {
                            aElementIdList.Add(aElement.Id);
                            continue;
                        }
                    }

                    if (!removeNulls)
                        aElementIdList.Add(null);
                }

                return aElementIdList;
            }

            /***************************************************/

            /// <summary>
            /// Utilities for Revit PlanarFace
            /// </summary>
            public static class PlanarFace
            {
                /// <summary>
                /// Checks if Revit PlanarFace is horizontal
                /// </summary>
                /// <param name="planarFace">Revit PlanarFace</param>
                /// <returns name="IsHorizontal">Is Horizontal</returns>
                /// <search>
                /// Utilis, IsHorizontal, Revit, Is Horizontal, PlanarFace, Planar Face
                /// </search>
                static public bool IsHorizontal(Autodesk.Revit.DB.PlanarFace planarFace)
                {
                    return XYZ.IsVertical(planarFace.FaceNormal);
                }
            }

            /// <summary>
            /// Utilities for Revit XYZ (Point or Vector)
            /// </summary>
            public static class XYZ
            {
                /// <summary>
                /// Checks if Revit Vector (XYZ) is Vertical
                /// </summary>
                /// <param name="xyz">Revit Vector</param>
                /// <returns name="IsVertical">Is Vertical</returns>
                /// <search>
                /// Utilis, IsVertical, Revit, Is Vertical, XYZ, Vector
                /// </search>
                static public bool IsVertical(Autodesk.Revit.DB.XYZ xyz)
                {
                    return Double.IsZero(xyz.X) && Double.IsZero(xyz.Y);
                }
            }

            /// <summary>
            /// Utilities for double values
            /// </summary>
            public static class Double
            {
                /// <summary>
                /// Checks if double value is zero. Method takes into account Machine epsilon error
                /// </summary>
                /// <param name="double">double value</param>
                /// <returns name="IsZero">IsZero</returns>
                /// <search>
                /// Utilis, IsZero, Revit, Is Zero, double
                /// </search>
                static public bool IsZero(double @double)
                {
                    return pEpsilon > Math.Abs(@double);
                }
            }
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
            /// <param name="builtInCategory">Revit BuiltInCategory</param>
            /// <returns name="BuildingElementType">BHoM BuildingElementType</returns>
            /// <search>
            /// Utilis, BHoM, GetBuildingElementType, Get BuildingElementType, BuiltInCategory, Revit
            /// </search>
            public static BuildingElementType? GetBuildingElementType(BuiltInCategory builtInCategory)
            {
                switch(builtInCategory)
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
            /// Copies Revit Identifiers to BHoMObject CustomData. Key: Utilis.ElementId Value: Element.Id.IntegerValue (storage type int), Key: Utilis.AdapterId Value: Element.UniqueId (storage type string)
            /// </summary>
            /// <param name="bHoMObject">BHoMObject</param>
            /// <param name="element">Revit Element</param>
            /// <search>
            /// Utilis, BHoM, CopyIdentifiers, Revit, Copy Identifiers, BHoMObject
            /// </search>
            public static void CopyIdentifiers(BHoMObject bHoMObject, Element element)
            {
                if (bHoMObject == null || element == null)
                    return;

                bHoMObject.CustomData.Add(ElementId, element.Id.IntegerValue);
                bHoMObject.CustomData.Add(AdapterId, element.UniqueId);
            }

            /// <summary>
            /// Removes Revit Identifiers from BHoMObject CustomData. Key: Utilis.ElementId, Key: Utilis.AdapterId
            /// </summary>
            /// <param name="bHoMObject">BHoMObject</param>
            /// <search>
            /// Utilis, BHoM, RemoveIdentifiers, Remove Identifiers, BHoMObject
            /// </search>
            public static void RemoveIdentifiers(BHoMObject bHoMObject)
            {
                if (bHoMObject == null)
                    return;
                
                bHoMObject.CustomData.Remove(AdapterId);
                bHoMObject.CustomData.Remove(ElementId);

            }

            /// <summary>
            /// Reads Revit UniqueId from BHoMObject CustomData. Key: Utilis.AdapterId
            /// </summary>
            /// <param name="bHoMObject">BHoMObject</param>
            /// <returns name="UniqueId">Revit UniqueId</returns>
            /// <search>
            /// Utilis, BHoM, GetUniqueId, Get UniqueId, BHoMObject
            /// </search>
            public static string GetUniqueId(BHoMObject bHoMObject)
            {
                if (bHoMObject == null)
                    return null;

                object aValue = null;
                if(bHoMObject.CustomData.TryGetValue(AdapterId, out aValue))
                {
                    if (aValue is string)
                        return (string)aValue;
                    else
                        return null;
                }

                return null;
            }

            /// <summary>
            /// Reads Revit ElementId from BHoMObject CustomData. Key: Utilis.ElementId
            /// </summary>
            /// <param name="bHoMObject">BHoMObject</param>
            /// <returns name="ElementId">Revit ElementId</returns>
            /// <search>
            /// Utilis, BHoM, GetElemenId, Get ElemenId, BHoMObject
            /// </search>
            public static ElementId GetElemenId(BHoMObject bHoMObject)
            {
                if (bHoMObject == null)
                    return null;

                object aValue = null;
                if (bHoMObject.CustomData.TryGetValue(ElementId, out aValue))
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
            /// Reads Revit UniqueIds from BHoMObjects CustomData. Key: Utilis.AdapterId
            /// </summary>
            /// <param name="bHoMObjects">BHoMObject collection</param>
            /// <param name="removeNulls">Removes nulls from result list</param>
            /// <returns name="UniqueIds">UniqueId List</returns>
            /// <search>
            /// Utilis, BHoM, GetUniqueIdList, Get UniqueId List, BHoMObject
            /// </search>
            public static List<string> GetUniqueIdList(IEnumerable<BHoMObject> bHoMObjects, bool removeNulls = true)
            {
                if (bHoMObjects == null)
                    return null;

                List<string> aUniqueIdList = new List<string>();
                foreach (BHoMObject aBHoMObject in bHoMObjects)
                {
                    string aUniqueId = GetUniqueId(aBHoMObject);
                    if (string.IsNullOrEmpty(aUniqueId) && removeNulls)
                        continue;

                    aUniqueIdList.Add(aUniqueId);
                }

                return aUniqueIdList;
            }

            /// <summary>
            /// Copy values from BHoMObject CustomData to Revit Element parameters
            /// </summary>
            /// <param name="bHoMObject">Source BHoMObject</param>
            /// <param name="element">Destination Revit Element</param>
            /// <search>
            /// Utilis, BHoM, CopyCustomData,  BHoMObject, Revit, Element, Copy CustomData
            /// </search>
            public static void CopyCustomData(BHoMObject bHoMObject, Element element)
            {
                if (bHoMObject == null || element == null)
                    return;

                foreach (Parameter aParameter in element.ParametersMap)
                {
                    if (bHoMObject.CustomData.ContainsKey(aParameter.Definition.Name))
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

                    bHoMObject.CustomData.Add(aParameter.Definition.Name, aValue);
                }
            }

            /// <summary>
            /// Gets BHoM class type for given Element
            /// </summary>
            /// <param name="element">Revit Element</param>
            /// <returns name="Type">BHoM class Type</returns>
            /// <search>
            /// Utilis, BHoM, GetType,  BHoMObject, Revit, Element, Get Type
            /// </search>
            public static System.Type GetType(Element element)
            {
                if (element is CeilingType)
                    return typeof(BuildingElementProperties);
                if (element is WallType)
                    return typeof(BuildingElementProperties);
                if (element is FloorType)
                    return typeof(BuildingElementProperties);
                if (element is RoofType)
                    return typeof(BuildingElementProperties);
                if (element.GetType().IsAssignableFrom(typeof(SpatialElement)))
                    return typeof(Space);
                if (element is Wall)
                    return typeof(BuildingElement);
                if (element is Ceiling)
                    return typeof(BuildingElement);
                if (element.GetType().IsAssignableFrom(typeof(RoofBase)))
                    return typeof(BuildingElement);
                if (element is Floor)
                    return typeof(BuildingElement);

                return null;
            }

            /***************************************************/
        }

        public static class Type
        {
            /***************************************************/
            /**** Public Methods                            ****/
            /***************************************************/

            public static bool IsAssignableFromByFullName(System.Type type, System.Type typeToCheck)
            {
                if (type == null || typeToCheck == null)
                    return false;

                return GetFullNames(typeToCheck).Contains(type.FullName);
            }

            /***************************************************/
            /**** Private Methods                            ****/
            /***************************************************/

            private static List<string> GetFullNames(System.Type type)
            {
                List<string> aResult = new List<string>();
                aResult.Add(type.FullName);
                if (type.BaseType != null)
                    aResult.AddRange(GetFullNames(type.BaseType));
                return aResult;
            }
        }


    }
}
