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
using Autodesk.Revit.DB.Structure;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Elements;

using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Window ToBHoMWindow(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Window aWindow = pullSettings.FindRefObject<Window>(familyInstance.Id.IntegerValue);
            if (aWindow != null)
                return aWindow;

            PlanarSurface aPlanarSurface = BH.Engine.Geometry.Create.PlanarSurface(Query.PolyCurve(familyInstance, pullSettings));
            aWindow = new Window()
            {
                Name = Query.FamilyTypeFullName(familyInstance),
                Location = aPlanarSurface

            };

            aWindow = Modify.SetIdentifiers(aWindow, familyInstance) as Window;
            if (pullSettings.CopyCustomData)
                aWindow = Modify.SetCustomData(aWindow, familyInstance, pullSettings.ConvertUnits) as Window;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aWindow);

            return aWindow;
        }

        /***************************************************/
    }
}