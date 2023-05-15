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

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        //[Description("Creates and returns a new Sheet in the current Revit file.")]
        //[Input("document", "The current Revit document to be processed.")]
        //[Input("sheetName", "Name of the new sheet.")]
        //[Input("sheetNumber", "Number of the new sheet.")]
        //[Input("titleBlockId", "The Title Block Id to be applied to the sheet.")]
        //[Output("newSheet", "The new sheet.")]
        public static IndependentTag IndependentTag(this Element elem, Document doc, View view, ElementId tagTypeId, XYZ tagPoint = null)
        {
            //ElementId catId = elem.Category.Id;
            //ElementId tagTypeId = taggedItemsCount > 1
            //    ? new ElementId(m_TypicalTagTypeByCategoryId[catId.IntegerValue])
            //    : m_TagTypeByCategoryId[catId];

            //rotateWithComponent = (doc.GetElement(tagTypeId) as FamilySymbol)
            //    .Family.get_Parameter(BuiltInParameter.FAMILY_ROTATE_WITH_COMPONENT)
            //    .AsInteger() == 1;

            if (tagPoint == null)
            {
                BoundingBoxXYZ eBox = elem.get_BoundingBox(view);
                tagPoint = (eBox.Max + eBox.Min) / 2;
            }

            return Autodesk.Revit.DB.IndependentTag.Create(doc, tagTypeId, view.Id, new Reference(elem), false, TagOrientation.Horizontal, tagPoint);
        }

        /***************************************************/
    }
}



