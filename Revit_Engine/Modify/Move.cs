/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Translates ModelInstance by given vector.")]
        [Input("modelInstance", "ModelInstance to be moved.")]
        [Input("vector", "Translation vector.")]
        [Output("modelInstance")]
        public static ModelInstance Move(this ModelInstance modelInstance, Vector vector)
        {
            if (modelInstance == null)
                return null;

            ModelInstance modInstance = modelInstance.ShallowClone();

            modInstance.Location = Geometry.Modify.Translate(modInstance.Location as dynamic, vector);
            
            return modInstance;
        }

        /***************************************************/
    }
}



