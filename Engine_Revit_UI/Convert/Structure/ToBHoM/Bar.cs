/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.SectionProperties;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Geometry.ShapeProfiles;
using BHG = BH.Engine.Geometry;
using BHS = BH.Engine.Structure;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<Bar> ToBHoMBar(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<Bar> bars = pullSettings.FindRefObjects<Bar>(familyInstance.Id.IntegerValue);
            if (bars != null)
                return bars;
            
            oM.Geometry.ICurve locationCurve = null;
            BH.oM.Geometry.Vector startOffset = new oM.Geometry.Vector { X = 0, Y = 0, Z = 0 };
            BH.oM.Geometry.Vector endOffset = new oM.Geometry.Vector { X = 0, Y = 0, Z = 0 };

            AnalyticalModelStick analyticalModel = familyInstance.GetAnalyticalModel() as AnalyticalModelStick;
            if (analyticalModel != null)
                analyticalModel.AnalyticalBarLocation(pullSettings, out locationCurve);

            if (locationCurve == null)
                familyInstance.FramingElementLocation(pullSettings, out locationCurve, out startOffset, out endOffset);
            else
                familyInstance.AnalyticalPullWarning();

            if (locationCurve == null)
                familyInstance.BarCurveNotFoundWarning();

            IMaterialFragment materialFragment = null;
            BH.oM.Physical.Materials.Material material = pullSettings.FindRefObject<oM.Physical.Materials.Material>(familyInstance.StructuralMaterialId.IntegerValue);
            if (material != null)
                materialFragment = material.GetMaterialProperties(oM.Adapters.Revit.Enums.Discipline.Structural).FirstOrDefault() as IMaterialFragment;
            else
            {
                string materialGrade = familyInstance.MaterialGrade();
                materialFragment = Query.LibraryMaterial(familyInstance.StructuralMaterialType, materialGrade);

                if (materialFragment == null)
                {
                    Compute.MaterialNotInLibraryNote(familyInstance);

                    ElementId materialId = familyInstance.StructuralMaterialId;
                    if (materialId.IntegerValue > 0)
                    {
                        Autodesk.Revit.DB.Material revitMaterial = familyInstance.Document.GetElement(materialId) as Autodesk.Revit.DB.Material;
                        if (revitMaterial != null)
                            materialFragment = revitMaterial.ToBHoMMaterialFragment(pullSettings);
                    }

                    if (materialFragment == null)
                    {
                        Compute.InvalidDataMaterialWarning(familyInstance);
                        
                        string name = String.Format("Unknown {0} Material", familyInstance.StructuralMaterialType);

                        //TODO: separate method UnknownMaterial(StructuralMaterialType)?
                        switch (familyInstance.StructuralMaterialType)
                        {
                            case StructuralMaterialType.Aluminum:
                                {
                                    materialFragment = new Aluminium() { Name = name };
                                    break;
                                }
                            case StructuralMaterialType.Concrete:
                            case StructuralMaterialType.PrecastConcrete:
                                {
                                    materialFragment = new Concrete() { Name = name };
                                    break;
                                }
                            case StructuralMaterialType.Wood:
                                {
                                    materialFragment = new Timber() { Name = name };
                                    break;
                                }
                            default:
                                {
                                    materialFragment = new Steel() { Name = name };
                                    break;
                                }
                        }
                    }
                }
            }

            string elementName = familyInstance.Name;
            string profileName = familyInstance.Symbol.Name;

            ISectionProperty property = BH.Engine.Library.Query.Match("SectionProperties", profileName) as ISectionProperty;

            if (property == null)
            {
                IProfile profile = familyInstance.Symbol.ToBHoMProfile(pullSettings);
                if (profile == null)
                    profile = familyInstance.BHoMFreeFormProfile(pullSettings);

                //TODO: this should be removed and null passed finally?
                if (profile == null)
                    profile = new FreeFormProfile(new List<oM.Geometry.ICurve>());

                if (profile.Edges.Count == 0)
                    familyInstance.Symbol.ConvertProfileFailedWarning();

                //TODO: shouldn't we have empty AluminiumSection and TimberSection at least?
                if (materialFragment is Concrete)
                    property = BH.Engine.Structure.Create.ConcreteSectionFromProfile(profile, materialFragment as Concrete, profileName);
                else if (materialFragment is Steel)
                    property = BH.Engine.Structure.Create.SteelSectionFromProfile(profile, materialFragment as Steel, profileName);
                else
                {
                    familyInstance.UnknownMaterialWarning();
                    property = BH.Engine.Structure.Create.SteelSectionFromProfile(profile, null, profileName);
                    property.Material = materialFragment;
                }
            }
            else
            {
                property = property.GetShallowClone() as ISectionProperty;
                property.Material = materialFragment;
                property.Name = profileName;
            }
            
            bars = new List<Bar>();

            if (locationCurve != null)
            {
                double rotation = familyInstance.FramingElementRotation(pullSettings);
                foreach (BH.oM.Geometry.Line line in BH.Engine.Geometry.Query.SubParts(BH.Engine.Geometry.Modify.ICollapseToPolyline(locationCurve, Math.PI / 12)))
                {
                    Bar bar = BH.Engine.Structure.Create.Bar(line, property, rotation);
                    bar.Offset = new oM.Structure.Offsets.Offset { Start = startOffset, End = endOffset };
                    bars.Add(bar);
                }
            }
            else
                bars.Add(BH.Engine.Structure.Create.Bar(null, null, property, 0));

            for (int i = 0; i < bars.Count; i++)
            {
                bars[i].Name = elementName;

                bars[i] = Modify.SetIdentifiers(bars[i], familyInstance) as Bar;
                if (pullSettings.CopyCustomData)
                    bars[i] = Modify.SetCustomData(bars[i], familyInstance, pullSettings.ConvertUnits) as Bar;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(bars[i]);
            }
            
            return bars;
        }

        /***************************************************/

        private static void AnalyticalBarLocation(this AnalyticalModelStick analyticalModel, PullSettings pullSettings, out oM.Geometry.ICurve locationCurve)
        {
            locationCurve = null;

            Curve curve = analyticalModel.GetCurve();
            if (curve == null)
                return;

            locationCurve = curve.ToBHoM(pullSettings);
        }

        /***************************************************/
    }
}