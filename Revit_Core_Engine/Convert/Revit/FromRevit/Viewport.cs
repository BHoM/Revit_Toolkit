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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Viewport to BH.oM.Adapters.Revit.Elements.Viewport.")]
        [Input("revitViewPort", "Revit Viewport to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("viewport", "BH.oM.Adapters.Revit.Elements.Viewport resulting from converting the input Revit Viewport.")]
        public static oM.Adapters.Revit.Elements.Viewport ViewportFromRevit(this Viewport revitViewPort, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.Viewport viewPort = refObjects.GetValue<oM.Adapters.Revit.Elements.Viewport>(revitViewPort.Id.IntegerValue);
            if (viewPort != null)
                return viewPort;

            oM.Geometry.Point location = revitViewPort.GetBoxCenter().PointFromRevit();
            string viewName = revitViewPort.get_Parameter(BuiltInParameter.VIEW_NAME).AsString();
            string sheetNumber = revitViewPort.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsString();

            viewPort = BH.Engine.Adapters.Revit.Create.Viewport(sheetNumber, viewName, location);

            ElementType elementType = revitViewPort.Document.GetElement(revitViewPort.GetTypeId()) as ElementType;
            if (elementType != null)
                viewPort.InstanceProperties = elementType.InstancePropertiesFromRevit(settings, refObjects);

            viewPort.Name = revitViewPort.Name;

            //Set identifiers, parameters & custom data
            viewPort.SetIdentifiers(revitViewPort);
            viewPort.CopyParameters(revitViewPort, settings.MappingSettings);
            viewPort.SetProperties(revitViewPort, settings.MappingSettings);

            refObjects.AddOrReplace(revitViewPort.Id, viewPort);
            return viewPort;
        }

        /***************************************************/
    }
}






