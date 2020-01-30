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

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Sheet ToBHoMSheet(this ViewSheet viewSheet, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Sheet sheet = pullSettings.FindRefObject<Sheet>(viewSheet.Id.IntegerValue);
            if (sheet != null)
                return sheet;

            sheet = BH.Engine.Adapters.Revit.Create.Sheet(viewSheet.Name, viewSheet.SheetNumber);

            ElementType elementType = viewSheet.Document.GetElement(viewSheet.GetTypeId()) as ElementType;
            if (elementType != null)
                sheet.InstanceProperties = ToBHoMInstanceProperties(elementType, pullSettings);

            sheet.Name = viewSheet.Name;

            sheet = Modify.SetIdentifiers(sheet, viewSheet) as Sheet;
            if (pullSettings.CopyCustomData)
                sheet = Modify.SetCustomData(sheet, viewSheet) as Sheet;

            sheet = sheet.UpdateValues(pullSettings, viewSheet) as Sheet;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(sheet);

            return sheet;
        }

        /***************************************************/
    }
}

