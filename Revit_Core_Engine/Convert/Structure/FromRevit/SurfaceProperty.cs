/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Reflection.Attributes;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.SurfaceProperties;
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

        [Description("Converts a Revit HostObjAttributes to an BH.oM.Structure.SurfaceProperties.ISurfaceProperty.")]
        [Input("hostObjAttributes", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("property", "BH.oM.Structure.SurfaceProperties.ISurfaceProperty resulting from converting the input Revit HostObjAttributes.")]
        public static ISurfaceProperty SurfacePropertyFromRevit(this HostObjAttributes hostObjAttributes, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return hostObjAttributes.SurfacePropertyFromRevit(null, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit HostObjAttributes to an BH.oM.Structure.SurfaceProperties.ISurfaceProperty.")]
        [Input("hostObjAttributes", "Revit HostObjAttributes to be converted.")]
        [Input("materialGrade", "Material grade extracted from the Revit element parent to the given HostObjAttributes, to be applied to the resultant BHoM ISurfaceProperty.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("property", "BH.oM.Structure.SurfaceProperties.ISurfaceProperty resulting from converting the input Revit HostObjAttributes.")]
        public static ISurfaceProperty SurfacePropertyFromRevit(this HostObjAttributes hostObjAttributes, string materialGrade = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            Document document = hostObjAttributes.Document;

            settings = settings.DefaultIfNull();

            string refId = hostObjAttributes.Id.ReferenceIdentifier(materialGrade);
            ConstantThickness property2D = refObjects.GetValue<ConstantThickness>(refId);
            if (property2D != null)
                return property2D;

            IMaterialFragment materialFragment = null;
            double thickness = 0;

            IList<CompoundStructureLayer> compoundStructure = hostObjAttributes.GetCompoundStructure()?.GetLayers();
            if (compoundStructure != null)
            {
                bool composite = false;
                bool nonStructuralLayers = false;

                foreach (CompoundStructureLayer csl in compoundStructure)
                {
                    if (csl.Function == MaterialFunctionAssignment.Structure)
                    {
                        if (thickness != 0)
                        {
                            composite = true;
                            thickness = 0;
                            break;
                        }

                        thickness = csl.Width.ToSI(UnitType.UT_Section_Dimension);

                        Material revitMaterial = document.GetElement(csl.MaterialId) as Material;
                        if (revitMaterial == null)
                            revitMaterial = hostObjAttributes.Category.Material;

                        materialFragment = revitMaterial.MaterialFragmentFromRevit(materialGrade, settings, refObjects);
                        if (materialFragment == null)
                        {
                            BH.Engine.Reflection.Compute.RecordWarning("There is a structural layer in wall/floor type without material assigned. A default empty material is returned. ElementId: " + hostObjAttributes.Id.IntegerValue.ToString());
                            materialFragment = Autodesk.Revit.DB.Structure.StructuralMaterialType.Undefined.EmptyMaterialFragment(materialGrade);
                        }
                    }
                    else if (csl.Function == MaterialFunctionAssignment.StructuralDeck)
                        BH.Engine.Reflection.Compute.RecordWarning(String.Format("A structural deck layer has been found in the Revit compound structure, but was ignored on convert. Revit ElementId: {0}", hostObjAttributes.Id));
                    else
                        nonStructuralLayers = true;
                }

                if (nonStructuralLayers)
                    BH.Engine.Reflection.Compute.RecordWarning(String.Format("Layers marked as nonstructural have been found in the Revit compound structure and were ignored on convert. Revit ElementId: {0}", hostObjAttributes.Id));

                if (composite)
                    hostObjAttributes.CompositePanelWarning();
                else if (thickness == 0)
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", hostObjAttributes.Id.IntegerValue));
            }
            else
                BH.Engine.Reflection.Compute.RecordWarning(String.Format("Revit panel type does not contain any layers (possibly it is a curtain panel). A BHoM panel type with zero thickness and no material is returned. Revit ElementId: {0}", hostObjAttributes.Id));

            oM.Structure.SurfaceProperties.PanelType panelType = oM.Structure.SurfaceProperties.PanelType.Undefined;
            if (hostObjAttributes is WallType)
                panelType = oM.Structure.SurfaceProperties.PanelType.Wall;
            else if (hostObjAttributes is FloorType)
                panelType = oM.Structure.SurfaceProperties.PanelType.Slab;

            property2D = new ConstantThickness { PanelType = panelType, Thickness = thickness, Material = materialFragment, Name = hostObjAttributes.Name };

            //Set identifiers, parameters & custom data
            property2D.SetIdentifiers(hostObjAttributes);
            property2D.CopyParameters(hostObjAttributes, settings.MappingSettings);
            property2D.SetProperties(hostObjAttributes, settings.MappingSettings);

            refObjects.AddOrReplace(refId, property2D);
            return property2D;
        }

        /***************************************************/
    }
}

