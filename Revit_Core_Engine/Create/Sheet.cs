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
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates and returns a new Floor Plan view in the current Revit file.")]
        [Input("document", "The current Revit document to be processed.")]
        [Input("sheetName", "Name of the new sheet.")]
        [Input("sheetNumber", "Number of the new sheet.")]
        [Input("viewTemplateId", "The Title Block Id to be applied to the sheet.")]
        [Output("viewSheet", "The new sheet.")]
        public static ViewSheet Sheet(this Document document, string sheetName, string sheetNumber, ElementId titleBlockId)
        {
            ViewSheet result = ViewSheet.Create(document, titleBlockId);

            if (!string.IsNullOrEmpty(sheetName))
            {
                try
                {
                    result.Name = sheetName;
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException)
                {
                    BH.Engine.Base.Compute.RecordWarning("There is already a sheet named '" + sheetName + "'." + " It has been named '" + result.Name + "' instead.");
                }
            }

            if (!string.IsNullOrEmpty(sheetNumber))
            {
                try
                {
                    result.SheetNumber = sheetNumber;
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException)
                {
                    BH.Engine.Base.Compute.RecordWarning("There is already a sheet with number '" + sheetNumber + "'." + " It has been named '" + result.SheetNumber + "' instead.");
                }
            }

            return result;
        }

        /***************************************************/
    }
}



