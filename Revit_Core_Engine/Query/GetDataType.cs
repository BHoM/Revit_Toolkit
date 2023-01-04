/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
#if (REVIT2018 || REVIT2019 || REVIT2020)

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("This method is defined by BHoM only for Revit Versions < 2021. Revit versions from 2021 onwards define an equivalent method with the same name as part of their API." +
                     "This BHoM implementation eliminates breaking changes between different Revit API versions. It returns the equivalent of UnitType from a Revit parameter Definition.")]
        [Input("definition", "Revit parameter Definition to extract the UnitType from.")]
        [Output("unitType", "UnitType extracted from the input Revit parameter Definition.")]
        public static UnitType GetDataType(this Definition definition)
        {
            return definition.UnitType;
        }

        /***************************************************/
#elif (REVIT2021)
        public static ForgeTypeId GetDataType(this Definition definition)
        {
            return definition.GetSpecTypeId();
        }
#endif
    }
}




