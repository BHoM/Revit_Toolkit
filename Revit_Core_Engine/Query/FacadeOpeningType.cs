/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using BH.oM.Facade.Elements;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Deducts facade opening type from a FamilyInstance representing a curtain panel.")]
        [Input("panel", "FamilyInstance representing a curtain panel.")]
        [Output("openingType", "Facade opening type deducted from the input curtain panel represented by a FamilyInstance.")]
        public static OpeningType FacadeOpeningType(this FamilyInstance panel)
        {
            BuiltInCategory category = (BuiltInCategory)panel.Category.Id.IntegerValue;
            if (category == Autodesk.Revit.DB.BuiltInCategory.OST_Windows)
                return BH.oM.Facade.Elements.OpeningType.Window;
            else if (category == Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels)
            {
                if (panel.IsTransparent())
                    return BH.oM.Facade.Elements.OpeningType.Window;
                else
                    return BH.oM.Facade.Elements.OpeningType.CurtainWallSpandrel;
            }
            else if (category == Autodesk.Revit.DB.BuiltInCategory.OST_Doors)
                return BH.oM.Facade.Elements.OpeningType.Door;
            else
                return BH.oM.Facade.Elements.OpeningType.Undefined;
        }

        /***************************************************/
    }
}

