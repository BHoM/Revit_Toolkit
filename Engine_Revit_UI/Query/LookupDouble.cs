/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.Collections.Generic;

using Autodesk.Revit.DB;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static double LookupDouble(this Element element, string parameterName, bool convertUnits = true)
        {
            double value = double.NaN;
            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = Convert.ToSI(value, p.Definition.UnitType);
            }
            return value;
        }

        /***************************************************/

        public static double LookupDouble(this Element element, BuiltInParameter builtInParameter, bool convertUnits = true)
        {
            double value = double.NaN;
            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = Convert.ToSI(value, p.Definition.UnitType);
            }
            return value;
        }

        /***************************************************/

        public static int LookupInteger(this Element element, BuiltInParameter builtInParameter)
        {
            int value = -1;

            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
                value = p.AsInteger();

            return value;
        }

        /***************************************************/

        public static ElementId LookupElementId(this Element element, BuiltInParameter builtInParameter)
        {
            ElementId value = new ElementId(-1);

            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
                value = p.AsElementId();

            return value;
        }

        /***************************************************/

        public static double LookupDouble(this Element element, IEnumerable<string> parameterNames, bool convertUnits = true)
        {
            double value = double.NaN;
            Parameter p = element.LookupParameter(parameterNames);
            if (p != null)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = Convert.ToSI(value, p.Definition.UnitType);
            }
            return value;
        }

        /***************************************************/

        public static string LookupString(this Element element, string parameterName)
        {
            Parameter p = element.LookupParameter(parameterName);

            if (p != null && p.HasValue)
            {
                switch (p.StorageType)
                {
                    case (StorageType.Double):
                        return UnitUtils.ConvertFromInternalUnits(p.AsDouble(), p.DisplayUnitType).ToString();
                    case (StorageType.Integer):
                        return p.AsInteger().ToString();
                    case (StorageType.String):
                        return p.AsString();
                    case (StorageType.ElementId):
                        return p.AsValueString();
                    default:
                        return null;
                }

            }
            return null;
        }

        /***************************************************/
    }
}