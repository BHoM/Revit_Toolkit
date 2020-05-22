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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Window WindowFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Window window = refObjects.GetValue<Window>(familyInstance.Id);
            if (window != null)
                return window;

            PolyCurve polycurve = familyInstance.PolyCurve(settings);
            if (polycurve == null)
                return null;

            window = new Window()
            {
                Name = Query.FamilyTypeFullName(familyInstance),
                Location = BH.Engine.Geometry.Create.PlanarSurface(polycurve)
            };
            
            ElementType elementType = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as ElementType;

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = familyInstance.Id.IntegerValue.ToString();
            originContext.TypeName = familyInstance.FamilyTypeFullName();
            originContext.SetProperties(familyInstance, settings.ParameterSettings);
            originContext.SetProperties(elementType, settings.ParameterSettings);
            window.Fragments.Add(originContext);

            //Set identifiers, parameters & custom data
            window.SetIdentifiers(familyInstance);
            window.SetCustomData(familyInstance, settings.ParameterSettings);
            window.SetProperties(familyInstance, settings.ParameterSettings);

            refObjects.AddOrReplace(familyInstance.Id, window);
            return window;
        }

        /***************************************************/

        public static Window WindowFromRevit(this Panel panel, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Window window = refObjects.GetValue<Window>(panel.Id.IntegerValue);
            if (window != null)
                return window;

            PolyCurve polycurve = panel.PolyCurve(settings);
            if (polycurve == null)
                return null;
            
            window = new Window()
            {
                Name = panel.FamilyTypeFullName(),
                Location = BH.Engine.Geometry.Create.PlanarSurface(polycurve)
            };
            
            ElementType elementType = panel.Document.GetElement(panel.GetTypeId()) as ElementType;

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = panel.Id.IntegerValue.ToString();
            originContext.TypeName = panel.FamilyTypeFullName();
            originContext.SetProperties(panel, settings.ParameterSettings);
            originContext.SetProperties(elementType, settings.ParameterSettings);
            window.Fragments.Add(originContext);

            //Set identifiers, parameters & custom data
            window.SetIdentifiers(panel);
            window.SetCustomData(panel, settings.ParameterSettings);
            window.SetProperties(panel, settings.ParameterSettings);

            refObjects.AddOrReplace(panel.Id, window);
            return window;
        }

        /***************************************************/
    }
}
