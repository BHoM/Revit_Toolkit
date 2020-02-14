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
using System;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Adapters.Revit.Elements.DraftingInstance ToBHoMDraftingInstance(this CurveElement curveElement, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.DraftingInstance draftingInstance = refObjects.GetValue<oM.Adapters.Revit.Elements.DraftingInstance>(curveElement.Id);
            if (draftingInstance != null)
                return draftingInstance;

            View view = curveElement.Document.GetElement(curveElement.OwnerViewId) as View;
            if (view == null)
                return null;

            oM.Adapters.Revit.Properties.InstanceProperties instanceProperties = (curveElement.LineStyle as GraphicsStyle).ToBHoMInstanceProperties(settings, refObjects) as oM.Adapters.Revit.Properties.InstanceProperties;
            draftingInstance = BH.Engine.Adapters.Revit.Create.DraftingInstance(instanceProperties, view.Name, curveElement.GeometryCurve.IToBHoM());
            draftingInstance.Name = curveElement.Name;

            //Set identifiers & custom data
            draftingInstance = draftingInstance.SetIdentifiers(curveElement) as oM.Adapters.Revit.Elements.DraftingInstance;
            draftingInstance = draftingInstance.SetCustomData(curveElement) as oM.Adapters.Revit.Elements.DraftingInstance;

            draftingInstance = draftingInstance.UpdateValues(settings, curveElement) as oM.Adapters.Revit.Elements.DraftingInstance;

            refObjects.AddOrReplace(curveElement.Id, draftingInstance);
            return draftingInstance;
        }

        /***************************************************/
        
        public static oM.Adapters.Revit.Elements.DraftingInstance ToBHoMDraftingInstance(this FilledRegion filledRegion, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.DraftingInstance draftingInstance = refObjects.GetValue<oM.Adapters.Revit.Elements.DraftingInstance>(filledRegion.Id);
            if (draftingInstance != null)
                return draftingInstance;

            View view = filledRegion.Document.GetElement(filledRegion.OwnerViewId) as View;
            if (view == null)
                return null;

            oM.Adapters.Revit.Properties.InstanceProperties instanceProperties = (filledRegion.Document.GetElement(filledRegion.GetTypeId()) as ElementType).ToBHoMInstanceProperties(settings, refObjects) as oM.Adapters.Revit.Properties.InstanceProperties;

            List<oM.Geometry.ICurve> curves = new List<oM.Geometry.ICurve>();
            foreach (CurveLoop loop in filledRegion.GetBoundaries())
            {
                curves.Add(loop.ToBHoM());
            }

            List<oM.Geometry.PlanarSurface> surfaces = BH.Engine.Geometry.Create.PlanarSurface(curves);
            if (surfaces.Count != 1)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("BHoM supports only filled regions consisting of single surfaces. The region could not be converted. Element Id: {0}", filledRegion.Id.IntegerValue));
                return null;
            }

            draftingInstance = BH.Engine.Adapters.Revit.Create.DraftingInstance(instanceProperties, view.Name, surfaces[0]);
            draftingInstance.Name = filledRegion.Name;

            //Set identifiers & custom data
            draftingInstance = draftingInstance.SetIdentifiers(filledRegion) as oM.Adapters.Revit.Elements.DraftingInstance;
            draftingInstance = draftingInstance.SetCustomData(filledRegion) as oM.Adapters.Revit.Elements.DraftingInstance;

            draftingInstance = draftingInstance.UpdateValues(settings, filledRegion) as oM.Adapters.Revit.Elements.DraftingInstance;

            refObjects.AddOrReplace(filledRegion.Id, draftingInstance);
            return draftingInstance;
        }

        /***************************************************/
    }
}

