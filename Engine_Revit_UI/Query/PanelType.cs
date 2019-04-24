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

using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static oM.Environment.Elements.PanelType? PanelType(this BuiltInCategory builtInCategory)
        {
            switch (builtInCategory)
            {
                case Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings:
                    return oM.Environment.Elements.PanelType.Ceiling;
                case Autodesk.Revit.DB.BuiltInCategory.OST_Floors:
                    return oM.Environment.Elements.PanelType.Floor;
                case Autodesk.Revit.DB.BuiltInCategory.OST_Roofs:
                    return oM.Environment.Elements.PanelType.Roof;
                case Autodesk.Revit.DB.BuiltInCategory.OST_Walls:
                    return oM.Environment.Elements.PanelType.Wall;
            }

            return null;
        }

        /***************************************************/

        public static oM.Environment.Elements.PanelType? PanelType(this Category Category)
        {
            if (Category == null)
                return null;

            return PanelType((BuiltInCategory)Category.Id.IntegerValue);
        }

        /***************************************************/
    }
}