/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Parameter SetParameter(this Parameter parameter, object value, Document document = null)
        {
            if (parameter == null || parameter.IsReadOnly)
                return null;

            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    double dbl = double.NaN;
                    if (value is double)
                    {
                        dbl = (double)value;
                    }
                    else if(value is int || value is byte || value is float|| value is long)
                    {
                        dbl = System.Convert.ToDouble(value);
                    }
                    else if (value is bool)
                    {
                        if ((bool)value)
                            dbl = 1.0;
                        else
                            dbl = 0.0;
                    }
                    else if (value is string)
                    {
                        if (!double.TryParse((string)value, out dbl))
                            dbl = double.NaN;
                    }

                    if (!double.IsNaN(dbl))
                    {
                        try
                        {
                            dbl = Convert.FromSI(dbl, parameter.Definition.UnitType);
                        }
                        catch
                        {
                            dbl = double.NaN;
                        }

                        if (!double.IsNaN(dbl))
                        {
                            parameter.Set(dbl);
                            return parameter;
                        }
                    }
                    break;
                case StorageType.ElementId:
                    ElementId elementID = null;
                    if (value is int)
                    {
                        elementID = new ElementId((int)value);
                    }
                    else if (value is string)
                    {
                        int num;
                        if (int.TryParse((string)value, out num))
                            elementID = new ElementId(num);
                    }
                    else if(value != null)
                    {
                        int num;
                        if (int.TryParse(value.ToString(), out num))
                            elementID = new ElementId(num);
                    }

                    if(elementID != null)
                    {
                        bool exists = false;
                        if (elementID == ElementId.InvalidElementId)
                            exists = true;

                        if(!exists)
                        {
                            if (document == null)
                            {
                                exists = true;
                            }
                            else
                            {
                                Element element = document.GetElement(elementID);
                                exists = element != null;
                            }
                                
                        }

                        if(exists)
                        {
                            parameter.Set(elementID);
                            return parameter;
                        }
                    }
                    break;
                case StorageType.Integer:
                    if (value is int)
                    {
                        parameter.Set((int)value);
                        return parameter;
                    }
                    else if(value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal)
                    {
                        parameter.Set(System.Convert.ToInt32(value));
                    }
                    else if (value is bool)
                    {
                        if ((bool)value)
                            parameter.Set(1);
                        else
                            parameter.Set(0);

                        return parameter;
                    }
                    else if (value is string)
                    {
                        int num = 0;
                        if (int.TryParse((string)value, out num))
                        {
                            parameter.Set(num);
                            return parameter;
                        }  
                    }
                    break;
                case StorageType.String:
                    if (value == null)
                    {
                        string val = null;
                        parameter.Set(val);
                        return parameter;
                    }
                    else if (value is string)
                    {
                        parameter.Set((string)value);
                        return parameter;
                    }
                    else
                    {
                        parameter.Set(value.ToString());
                        return parameter;
                    }
            }

            return null;
        }

        /***************************************************/

        public static bool TrySetParameter(this Element familyInstance, string parameterName, double value)
        {
            Parameter parameter = familyInstance.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Double)
            {
                parameter.Set(value);
                return true;
            }

            return false;
        }

        /***************************************************/

        public static bool TrySetParameter(this Element familyInstance, BuiltInParameter builtInParam, double value)
        {
            Parameter parameter = familyInstance.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Double)
            {
                parameter.Set(value);
                return true;
            }

            return false;
        }

        /***************************************************/

        public static bool TrySetParameter(this Element familyInstance, string parameterName, int value)
        {
            Parameter parameter = familyInstance.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Integer)
            {
                parameter.Set(value);
                return true;
            }

            return false;
        }

        /***************************************************/

        public static bool TrySetParameter(this Element familyInstance, BuiltInParameter builtInParam, int value)
        {
            Parameter parameter = familyInstance.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Integer)
            {
                parameter.Set(value);
                return true;
            }

            return false;
        }

        /***************************************************/

        public static bool TrySetParameter(this Element familyInstance, string parameterName, string value)
        {
            Parameter parameter = familyInstance.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.String)
            {
                parameter.Set(value);
                return true;
            }

            return false;
        }

        /***************************************************/

        public static bool TrySetParameter(this Element familyInstance, BuiltInParameter builtInParam, string value)
        {
            Parameter parameter = familyInstance.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.String)
            {
                parameter.Set(value);
                return true;
            }

            return false;
        }

        /***************************************************/

        public static bool TrySetParameter(this Element familyInstance, string parameterName, ElementId value)
        {
            Parameter parameter = familyInstance.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.ElementId)
            {
                parameter.Set(value);
                return true;
            }

            return false;
        }

        /***************************************************/

        public static bool TrySetParameter(this Element familyInstance, BuiltInParameter builtInParam, ElementId value)
        {
            Parameter parameter = familyInstance.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.ElementId)
            {
                parameter.Set(value);
                return true;
            }

            return false;
        }

        /***************************************************/
    }
}