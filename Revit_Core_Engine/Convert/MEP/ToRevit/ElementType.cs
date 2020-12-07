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
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Autodesk.Revit.DB.Mechanical.DuctType ToRevitElementType(this oM.MEP.System.SectionProperties.DuctSectionProperty property, Document document, IEnumerable<BuiltInCategory> categories = null, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (property == null || document == null)
                return null;

            Autodesk.Revit.DB.Mechanical.DuctType elementType = refObjects.GetValue<Autodesk.Revit.DB.Mechanical.DuctType>(document, property.BHoM_Guid);
            if (elementType != null)
                return elementType;

            settings = settings.DefaultIfNull();

            elementType = property.ElementType(document, categories, settings) as Autodesk.Revit.DB.Mechanical.DuctType;
            if (elementType != null)
                return elementType;

            List<Autodesk.Revit.DB.Mechanical.DuctType> ductTypes = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.DuctType)).Cast<Autodesk.Revit.DB.Mechanical.DuctType>().ToList();

            if (property.SectionProfile.ElementProfile is BH.oM.Spatial.ShapeProfiles.BoxProfile)
                elementType = ductTypes.FirstOrDefault(x => x.FamilyName == "Rectangular Duct");
            else if (property.SectionProfile.ElementProfile is BH.oM.Spatial.ShapeProfiles.TubeProfile)
                elementType = ductTypes.FirstOrDefault(x => x.FamilyName == "Round Duct");
            // Add more shapeProfiles when available

            if (elementType != null)
            {
                BH.Engine.Reflection.Compute.RecordNote($"Duct is being pushed as the first type available in the Revit model, in this case {elementType.Name}.");
            }
            else
                return null;

            // Copy parameters from BHoM object to Revit element
            elementType.CopyParameters(property, settings);

            refObjects.AddOrReplace(property, elementType);
            return elementType;
        }

        /***************************************************/

        public static Autodesk.Revit.DB.Plumbing.PipeType ToRevitElementType(this oM.MEP.System.SectionProperties.PipeSectionProperty property, Document document, IEnumerable<BuiltInCategory> categories = null, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (property == null || document == null)
                return null;

            Autodesk.Revit.DB.Plumbing.PipeType elementType = refObjects.GetValue<Autodesk.Revit.DB.Plumbing.PipeType>(document, property.BHoM_Guid);
            if (elementType != null)
                return elementType;

            settings = settings.DefaultIfNull();

            elementType = property.ElementType(document, categories, settings) as Autodesk.Revit.DB.Plumbing.PipeType;
            if (elementType != null)
                return elementType;

            List<Autodesk.Revit.DB.Plumbing.PipeType> pipeTypes = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Plumbing.PipeType)).Cast<Autodesk.Revit.DB.Plumbing.PipeType>().ToList();

            if (property.SectionProfile.ElementProfile is BH.oM.Spatial.ShapeProfiles.TubeProfile)
                elementType = pipeTypes.FirstOrDefault(x => x.FamilyName == "Pipe Types");

            if (elementType != null)
            {
                BH.Engine.Reflection.Compute.RecordNote($"Pipe is being pushed as the first type available in the Revit model, in this case {elementType.Name}.");
            }
            else
                return null;

            // Copy parameters from BHoM object to Revit element
            elementType.CopyParameters(property, settings);

            refObjects.AddOrReplace(property, elementType);
            return elementType;
        }

        /***************************************************/

        public static Autodesk.Revit.DB.Electrical.CableTrayType ToRevitElementType(this oM.MEP.System.SectionProperties.CableTraySectionProperty property, Document document, IEnumerable<BuiltInCategory> categories = null, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (property == null || document == null)
                return null;

            Autodesk.Revit.DB.Electrical.CableTrayType elementType = refObjects.GetValue<Autodesk.Revit.DB.Electrical.CableTrayType>(document, property.BHoM_Guid);
            if (elementType != null)
                return elementType;

            settings = settings.DefaultIfNull();

            elementType = property.ElementType(document, categories, settings) as Autodesk.Revit.DB.Electrical.CableTrayType;
            if (elementType != null)
                return elementType;

            List<Autodesk.Revit.DB.Electrical.CableTrayType> trayTypes = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Electrical.CableTrayType)).Cast<Autodesk.Revit.DB.Electrical.CableTrayType>().ToList();

            if (property.SectionProfile.ElementProfile is BH.oM.Spatial.ShapeProfiles.BoxProfile)
                elementType = trayTypes.FirstOrDefault(x => x.FamilyName == "Cable Tray with Fittings");

            if (elementType != null)
            {
                BH.Engine.Reflection.Compute.RecordNote($"CableTray is being pushed as the first type available in the Revit model, in this case {elementType.Name}.");
            }
            else
                return null;

            // Copy parameters from BHoM object to Revit element
            elementType.CopyParameters(property, settings);

            refObjects.AddOrReplace(property, elementType);
            return elementType;
        }

        /***************************************************/
    }
}
