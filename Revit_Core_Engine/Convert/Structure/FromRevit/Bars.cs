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
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Spatial.ShapeProfiles;
using BH.oM.Structure.Elements;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.SectionProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilyInstance to a collection of BH.oM.Structure.Elements.Bars.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("bars", "Collection of BH.oM.Structure.Elements.Bars resulting from converting the input Revit FamilyInstance.")]
        public static List<Bar> BarsFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<Bar> bars = refObjects.GetValues<Bar>(familyInstance.Id);
            if (bars != null)
                return bars;
            
            // Get bar curve
            List<oM.Geometry.ICurve> locationCurves = null;
#if (REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021 || REVIT2022)
            AnalyticalModelStick analyticalModel = familyInstance.GetAnalyticalModel() as AnalyticalModelStick;
            if (analyticalModel != null)
            {
                IList<Curve> curves = analyticalModel.GetCurves(AnalyticalCurveType.ActiveCurves);
                if (curves != null && curves.Count != 0)
                    locationCurves = curves.Select(x => x.IFromRevit()).ToList();
            }
#else
            Document doc = familyInstance.Document;
            AnalyticalToPhysicalAssociationManager manager = AnalyticalToPhysicalAssociationManager.GetAnalyticalToPhysicalAssociationManager(doc);
            AnalyticalMember analyticalMember = doc.GetElement(manager.GetAssociatedElementId(familyInstance.Id)) as AnalyticalMember;
            Curve curve = analyticalMember?.GetCurve();
            if (curve != null)
                locationCurves = new List<ICurve> { curve.IFromRevit() };
#endif

            if (locationCurves != null)
                familyInstance.AnalyticalPullWarning();
            else
                locationCurves = new List<ICurve> { familyInstance.LocationCurve(settings) };

            // Get bar material
            ElementId structuralMaterialId = familyInstance.StructuralMaterialId;
            if (structuralMaterialId.IntegerValue < 0)
                structuralMaterialId = familyInstance.Symbol.LookupParameterElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

            Material revitMaterial = familyInstance.Document.GetElement(structuralMaterialId) as Material;
            if (revitMaterial == null)
                revitMaterial = familyInstance.Category.Material;
            
            // Get material grade
            string materialGrade = familyInstance.MaterialGrade(settings);

            // Find material fragment: convert the material assigned to the element, if that returns null try finding a material in the library, based on the material type assigned to the family.
            IMaterialFragment materialFragment = revitMaterial.MaterialFragmentFromRevit(materialGrade, settings, refObjects);
            if (materialFragment == null)
                materialFragment = familyInstance.StructuralMaterialType.LibraryMaterial(materialGrade);

            // If material fragment could not be found create an empty one and raise a warning further down the line.
            bool materialFound = materialFragment != null;

            if (materialFragment == null)
                materialFragment = familyInstance.StructuralMaterialType.EmptyMaterialFragment(materialGrade);

            // Get bar profile and create property
            string profileName = familyInstance.Symbol.Name;
            ISectionProperty property = BH.Engine.Library.Query.Match("Structure\\SectionProperties", profileName) as ISectionProperty;

            if (property == null)
            {
                IProfile profile = familyInstance.Symbol.ProfileFromRevit(settings, refObjects);

                //TODO: this should be removed and null passed finally?
                if (profile == null)
                    profile = new FreeFormProfile(new List<oM.Geometry.ICurve>());

                if (profile.Edges.Count == 0)
                    familyInstance.Symbol.ConvertProfileFailedWarning();

                if (!materialFound)
                    Compute.InvalidDataMaterialWarning(familyInstance);

                property = BH.Engine.Structure.Create.SectionPropertyFromProfile(profile, materialFragment, profileName);
            }
            else
            {
                property = property.ShallowClone();

                if (!materialFound)
                    BH.Engine.Base.Compute.RecordNote($"A matching section was found in the library. No valid material was defined in Revit, so the default material for this section was used. Revit ElementId: {familyInstance.Id.IntegerValue}");
                else
                    property.Material = materialFragment;

                property.Name = profileName;
            }
            
            // Create linear bars
            bars = new List<Bar>();
            if (locationCurves != null && locationCurves.Count != 0)
            {
                foreach (ICurve locationCurve in locationCurves)
                {
                    if (locationCurve is NurbsCurve)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Could not pull location of at least part of an Element because its location type is not supported: NurbsCurve. ElementId: {familyInstance.Id}");
                        bars.Add(new Bar { SectionProperty = property, OrientationAngle = 0 });
                        continue;
                    }

                    double rotation = familyInstance.OrientationAngle(settings);
                    foreach (BH.oM.Geometry.Line line in locationCurve.ICollapseToPolyline(Math.PI / 12).SubParts())
                    {
                        bars.Add(BH.Engine.Structure.Create.Bar(line, property, rotation));
                    }
                }
            }
            else
                bars.Add(new Bar { SectionProperty = property, OrientationAngle = 0 });

            for (int i = 0; i < bars.Count; i++)
            {
                bars[i].Name = familyInstance.Name;

                //Set identifiers, parameters & custom data
                bars[i].SetIdentifiers(familyInstance);
                bars[i].CopyParameters(familyInstance, settings.MappingSettings);
                bars[i].SetProperties(familyInstance, settings.MappingSettings);
            }

            refObjects.AddOrReplace(familyInstance.Id, bars);
            return bars;
        }

        /***************************************************/
    }
}



