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

using BH.oM.Base;
using BH.oM.Adapters.Revit.Properties;
using System.ComponentModel;
using BH.oM.Geometry;

namespace BH.oM.Adapters.Revit.Elements
{
    [Description("A wrapper BHoM type for Elevation Set, used to create or update Revit Elevations (on Push) and represent them as BHoMObjects (on Pull).")]
    public class ElevationSet : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Location point of the elevation marker.")]
        public virtual Point MarkerLocation { get; set; } = null;

        [Description("Id of the view that elevation set is referenced to.")]
        public virtual int ReferenceViewPlanId { get; set; } = 0;

        [Description("Elevation View that is oriented to the left.")]
        public virtual ElevationView LeftElevation { get; set; } = null;

        [Description("Elevation View that is oriented to the right.")]
        public virtual ElevationView RightElevation { get; set; } = null;

        [Description("Elevation View that is oriented to the top.")]
        public virtual ElevationView BottomElevation { get; set; } = null;

        [Description("Elevation View that is oriented to the bottom.")]
        public virtual ElevationView TopElevation { get; set; } = null;


        /***************************************************/
    }
}






