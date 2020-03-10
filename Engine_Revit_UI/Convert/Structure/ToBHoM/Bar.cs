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
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.Structure.Elements;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.SectionProperties;
using System;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<Bar> ToBHoMBar(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<Bar> bars = refObjects.GetValues<Bar>(familyInstance.Id);
            if (bars != null)
                return bars;
            
            // Get bar curve
            oM.Geometry.ICurve locationCurve = null;
            AnalyticalModelStick analyticalModel = familyInstance.GetAnalyticalModel() as AnalyticalModelStick;
            if (analyticalModel != null)
            {
                Curve curve = analyticalModel.GetCurve();
                if (curve != null)
                    locationCurve = curve.IToBHoM();
            }

            if (locationCurve != null)
                familyInstance.AnalyticalPullWarning();
            else
                locationCurve = familyInstance.FramingElementLocation(settings);

            // Get bar material
            string materialGrade = familyInstance.MaterialGrade();
            IMaterialFragment materialFragment = familyInstance.StructuralMaterialType.LibraryMaterial(materialGrade);

            if (materialFragment == null)
            {
                // Check if an instance or type Structural Material parameter exists.
                ElementId structuralMaterialId = familyInstance.StructuralMaterialId;
                if (structuralMaterialId.IntegerValue < 0)
                    structuralMaterialId = familyInstance.Symbol.LookupParameterElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

                materialFragment = (familyInstance.Document.GetElement(structuralMaterialId) as Material).ToBHoMMaterialFragment(null, settings, refObjects);
            }

            if (materialFragment == null)
            {
                Compute.InvalidDataMaterialWarning(familyInstance);
                materialFragment = familyInstance.StructuralMaterialType.EmptyMaterialFragment();
            }
            
            // Get bar profile and create property
            string profileName = familyInstance.Symbol.Name;
            ISectionProperty property = BH.Engine.Library.Query.Match("SectionProperties", profileName) as ISectionProperty;

            if (property == null)
            {
                IProfile profile = familyInstance.Symbol.ToBHoMProfile(settings, refObjects);
                if (profile == null)
                    profile = familyInstance.BHoMFreeFormProfile(settings, refObjects);

                //TODO: this should be removed and null passed finally?
                if (profile == null)
                    profile = new FreeFormProfile(new List<oM.Geometry.ICurve>());

                if (profile.Edges.Count == 0)
                    familyInstance.Symbol.ConvertProfileFailedWarning();

                property = BH.Engine.Structure.Create.SectionPropertyFromProfile(profile, materialFragment, profileName);
            }
            else
            {
                property = property.GetShallowClone() as ISectionProperty;
                property.Material = materialFragment;
                property.Name = profileName;
            }
            
            // Create linear bars
            bars = new List<Bar>();
            if (locationCurve != null)
            {
                double rotation = familyInstance.FramingElementRotation(settings);
                foreach (BH.oM.Geometry.Line line in locationCurve.ICollapseToPolyline(Math.PI / 12).SubParts())
                {
                    bars.Add(BH.Engine.Structure.Create.Bar(line, property, rotation));
                }
            }
            else
                bars.Add(BH.Engine.Structure.Create.Bar(null, null, property, 0));

            for (int i = 0; i < bars.Count; i++)
            {
                bars[i].Name = familyInstance.Name;

                //Set identifiers & custom data
                bars[i].SetIdentifiers(familyInstance);
                bars[i].SetCustomData(familyInstance);
            }

            refObjects.AddOrReplace(familyInstance.Id, bars);
            return bars;
        }

        /***************************************************/
    }
}
