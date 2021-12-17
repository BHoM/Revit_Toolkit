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

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds a Revit document hosting the given link document.")]
        [Input("linkDocument", "Revit link document to find the host for.")]
        [Output("hostDocument", "Revit document hosting the input link document.")]
        public static Document HostDocument(this Document linkDocument)
        {
            if (!linkDocument.IsLinked)
            {
                BH.Engine.Base.Compute.RecordWarning("The document is not a link document, therefore it cannot have a host document. The document itself is returned.");
                return linkDocument;
            }

            UIApplication uiApp = new UIApplication(linkDocument.Application);
            return uiApp.ActiveUIDocument.Document;
        }

        /***************************************************/
    }
}
