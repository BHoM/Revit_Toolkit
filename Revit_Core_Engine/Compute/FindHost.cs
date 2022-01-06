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
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.Engine.Spatial;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Reflection.Attributes;
using BH.oM.Revit;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Looks for a Revit element that geometrically contains the given BHoM object.")]
        [Input("bHoMObject", "BHoM object to find the containing element for.")]
        [Input("document", "Revit document to be searched for the containing element.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("host", "First Revit element that contains the input BHoM object.")]
        public static Element IFindHost(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            if (bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Host element could not be found for a null BHoM object.");
                return null;
            }

            return FindHost(bHoMObject as dynamic, document, settings);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Looks for a Revit element that geometrically contains the given builders work Opening.")]
        [Input("opening", "Builders work Opening to find the containing element for.")]
        [Input("document", "Revit document to be searched for the containing element.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("host", "First Revit element that contains the input builders work Opening.")]
        public static Element FindHost(this BH.oM.Architecture.BuildersWork.Opening opening, Document document, RevitSettings settings = null)
        {
            BuiltInCategory[] hostCategories = new BuiltInCategory[] { BuiltInCategory.OST_Floors, BuiltInCategory.OST_Walls, BuiltInCategory.OST_Roofs };
            return opening.FindHost(document, hostCategories, settings);
        }

        /***************************************************/

        [Description("Looks for a Revit element that geometrically contains the given IElement0D.")]
        [Input("element", "IElement0D to find the containing element for.")]
        [Input("document", "Revit document to be searched for the containing element.")]
        [Input("categories", "Revit categories to be taken into account when performing the search. If null, elements of all categories will be checked.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("host", "First Revit element that contains the input IElement0D.")]
        public static Element FindHost(this IElement0D element, Document document, IEnumerable<BuiltInCategory> categories, RevitSettings settings = null)
        {
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Host element could not be found for a null BHoM object.");
                return null;
            }

            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Host element could not be found in a null Revit document.");
                return null;
            }

            settings = settings.DefaultIfNull();

            XYZ location = element.IGeometry()?.ToRevit();
            if (location == null)
            {
                BH.Engine.Reflection.Compute.RecordError("The input BHoM object does not have a valid location.");
                return null;
            }

            Document hostDoc = document;
            string linkName = (element as IBHoMObject)?.FindFragment<RevitHostFragment>()?.LinkDocument;

            if (!string.IsNullOrWhiteSpace(linkName))
            {
                RevitLinkInstance linkInstance = hostDoc.LinkInstance(linkName);
                hostDoc = linkInstance?.Document;
                if (hostDoc == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("The link document declared in the host information of the input BHoM object does not exist.");
                    return null;
                }

                location = linkInstance.GetTotalTransform().Inverse.OfPoint(location);
            }

            return location.ContainingElement(hostDoc, categories, settings);        
        }


        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/

        private static Element FindHost(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            return null;
        }

        /***************************************************/
    }
}

