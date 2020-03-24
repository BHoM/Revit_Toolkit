/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using BH.oM.Environment.Fragments;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Deprecated("3.1", "This method is a duplicate of SetProperty.")]
        [Description("Sets Family Type name for Environment Context Properties")]
        [Input("originContextFragment", "Origin Context Properties")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("OriginContextFragment")]
        public static OriginContextFragment SetFamilyTypeName(this OriginContextFragment originContextFragment, string familyTypeName)
        {
            if (originContextFragment == null)
                return null;

            OriginContextFragment originContext = new OriginContextFragment();
            originContext.Description = originContextFragment.Description;
            originContext.ElementID = originContextFragment.ElementID;
            originContext.TypeName = familyTypeName;

            return originContext;
        }

        /***************************************************/
    }
}

