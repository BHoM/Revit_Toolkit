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

using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Normalises the angle to fall within the range between -Pi and +Pi.")]
        [Input("orientationAngle", "Angle to be normalised.")]
        [Output("normalised", "Angle normalised to fall within the range between -Pi and +Pi.")]
        public static double NormalizeAngleDomain(this double orientationAngle)
        {
            orientationAngle = orientationAngle % (2 * Math.PI);

            if (orientationAngle < -Math.PI)
                orientationAngle += Math.PI * 2;
            else if (orientationAngle > Math.PI)
                orientationAngle -= Math.PI * 2;

            return orientationAngle;
        }

        /***************************************************/
    }
}





