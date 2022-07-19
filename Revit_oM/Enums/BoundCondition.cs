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

using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Enums
{
    /***************************************************/

    [Description("An enumerator defining possible bound condition of a Revit room/space/area. " +
        "Below is the meaning of each condition for rooms. The same applies to spaces and areas." +
        "- Unplaced: https://help.autodesk.com/view/RVT/2022/ENU/?guid=GUID-BC1DC181-B6D0-4479-8385-363A9EE5E75E" +
        "- Not enclosed: https://help.autodesk.com/view/RVT/2022/ENU/?guid=GUID-1AEDF540-7CB3-4CAB-885A-ACDF70154312" +
        "- Redundant: https://help.autodesk.com/view/RVT/2022/ENU/?guid=GUID-0DF409DD-3BBD-4488-B544-D075D1807747" +
        "- Bounded (fully bounded on all sides by bounding elements): https://help.autodesk.com/view/RVT/2022/ENU/?guid=GUID-241430FC-8084-43A1-AA3A-681B2883B0FC")]
    public enum BoundCondition
    {
        Bounded,
        Unplaced,
        NotEnclosed,
        Redundant,
    }

    /***************************************************/
}


