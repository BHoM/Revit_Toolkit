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
using BH.oM.MEP.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Structure.Elements;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.SectionProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert wires in the model to their corresponding BHoM objects.")]
        [Input("Autodesk.Revit.DB.FamilyInstance", "Revit family instance.")]
        [Input("BH.oM.Adapters.Revit.Settings.RevitSettings", "Revit settings.")]
        [Input("Dictionary<string, List<IBHoMObject>>", "Referenced objects.")]
        [Output("List<Wire>", "Revit wires represented as BHoM objects.")]
        public static List<Wire> WireFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<Wire> wires = refObjects.GetValues<Wire>(familyInstance.Id);
            if (wires != null)
                return wires;

            // Get wire curve
            oM.Geometry.ICurve locationCurve = null;
            AnalyticalModelStick analyticalModel = familyInstance.GetAnalyticalModel() as AnalyticalModelStick;
            if (analyticalModel != null)
            {
                Curve curve = analyticalModel.GetCurve();
                if (curve != null)
                    locationCurve = curve.IFromRevit();
            }

            if (locationCurve != null)
                familyInstance.AnalyticalPullWarning();
            else
                locationCurve = familyInstance.LocationCurve(settings);

            // Get wire material
            string materialGrade = familyInstance.MaterialGrade(settings);
            IMaterialFragment materialFragment = familyInstance.StructuralMaterialType.LibraryMaterial(materialGrade);

            if (materialFragment == null)
            {
                // Check if an instance or type Structural Material parameter exists.
                ElementId structuralMaterialId = familyInstance.StructuralMaterialId;
                if (structuralMaterialId.IntegerValue < 0)
                    structuralMaterialId = familyInstance.Symbol.LookupParameterElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

                materialFragment = (familyInstance.Document.GetElement(structuralMaterialId) as Material).MaterialFragmentFromRevit(null, settings, refObjects);
            }

            if (materialFragment == null)
            {
                Compute.InvalidDataMaterialWarning(familyInstance);
                materialFragment = familyInstance.StructuralMaterialType.EmptyMaterialFragment();
            }

            // Get wire profile and create property
            string profileName = familyInstance.Symbol.Name;
            ISectionProperty property = BH.Engine.Library.Query.Match("SectionProperties", profileName) as ISectionProperty;

            if (property == null)
            {
                IProfile profile = familyInstance.Symbol.ProfileFromRevit(settings, refObjects);

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

            // List wires
            wires = new List<Wire>();
            if (locationCurve != null)
            {
                //TODO: check category of familyInstance to recognize which rotation query to use
                double rotation = familyInstance.OrientationAngle(settings);
                foreach (BH.oM.Geometry.Line line in locationCurve.ICollapseToPolyline(Math.PI / 12).SubParts())
                {
                    //wires.Add(BH.Engine.Environment.Create.Wire(line, property, rotation));
                    Wire wire = new Wire();
                    wire.WireSegments =
                    wires.Add(wire);
                }
            }
            else
                wires.Add(new Wire());

            for (int i = 0; i < wires.Count; i++)
            {
                wires[i].Name = familyInstance.Name;

                //Set identifiers, parameters & custom data
                wires[i].SetIdentifiers(familyInstance);
                wires[i].CopyParameters(familyInstance, settings.ParameterSettings);
                wires[i].SetProperties(familyInstance, settings.ParameterSettings);
            }

            refObjects.AddOrReplace(familyInstance.Id, wires);
            return wires;
        }

        /***************************************************/
    }
}