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

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Adapters.Revit.Elements.Viewport ToBHoMViewport(this Viewport viewport, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Adapters.Revit.Elements.Viewport aViewport = pullSettings.FindRefObject<oM.Adapters.Revit.Elements.Viewport>(viewport.Id.IntegerValue);
            if (aViewport != null)
                return aViewport;

            oM.Geometry.Point aLocation = viewport.GetBoxCenter().ToBHoM(pullSettings);
            string aViewName = viewport.get_Parameter(BuiltInParameter.VIEW_NAME).AsString();
            string aSheetNumber = viewport.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsString();

            aViewport = BH.Engine.Adapters.Revit.Create.Viewport(aSheetNumber, aViewName, aLocation);

            aViewport.Name = viewport.Name;

            aViewport = Modify.SetIdentifiers(aViewport, viewport) as oM.Adapters.Revit.Elements.Viewport;
            if (pullSettings.CopyCustomData)
                aViewport = Modify.SetCustomData(aViewport, viewport, pullSettings.ConvertUnits) as oM.Adapters.Revit.Elements.Viewport;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aViewport);

            return aViewport;
        }

        /***************************************************/
    }
}
