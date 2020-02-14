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
            
            oM.Geometry.ICurve locationCurve = null;
            BH.oM.Geometry.Vector startOffset = new oM.Geometry.Vector { X = 0, Y = 0, Z = 0 };
            BH.oM.Geometry.Vector endOffset = new oM.Geometry.Vector { X = 0, Y = 0, Z = 0 };

            AnalyticalModelStick analyticalModel = familyInstance.GetAnalyticalModel() as AnalyticalModelStick;
            if (analyticalModel != null)
                analyticalModel.AnalyticalBarLocation(out locationCurve, settings);

            if (locationCurve != null)
                familyInstance.AnalyticalPullWarning();
            else
            {
                familyInstance.FramingElementLocation(out locationCurve, out startOffset, out endOffset, settings);

                if (locationCurve == null)
                    familyInstance.BarCurveNotFoundWarning();
                else
                {
                    //TODO: wrap it into single method both in FramingElement and Bar
                    //TODO: for nonlinear bars this should be actual offset, not translation?
                    double startOffsetLength = BH.Engine.Geometry.Query.Length(startOffset);
                    double endOffsetLength = BH.Engine.Geometry.Query.Length(endOffset);
                    if (startOffsetLength > BH.oM.Geometry.Tolerance.Distance || endOffsetLength > BH.oM.Geometry.Tolerance.Distance)
                    {
                        Transform transform = familyInstance.GetTotalTransform();
                        //if (BH.Engine.Geometry.Query.Distance(startOffset, endOffset) <= BH.oM.Geometry.Tolerance.Distance)
                        //{
                        //    BH.oM.Geometry.Vector yOffset = new BH.oM.Geometry.Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
                        //    BH.oM.Geometry.Vector zOffset = new BH.oM.Geometry.Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
                        //    locationCurve = BHG.Modify.Translate(locationCurve as dynamic, yOffset - zOffset);
                        //}
                        if (locationCurve is BH.oM.Geometry.Line)
                        {
                            BH.oM.Geometry.Line l = locationCurve as BH.oM.Geometry.Line;
                            BH.oM.Geometry.Vector yOffsetStart = new BH.oM.Geometry.Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
                            BH.oM.Geometry.Vector zOffsetStart = new BH.oM.Geometry.Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
                            BH.oM.Geometry.Vector yOffsetEnd = new BH.oM.Geometry.Vector { X = transform.BasisY.X * endOffset.Y, Y = transform.BasisY.Y * endOffset.Y, Z = transform.BasisY.Z * endOffset.Y };
                            BH.oM.Geometry.Vector zOffsetEnd = new BH.oM.Geometry.Vector { X = transform.BasisZ.X * endOffset.Z, Y = transform.BasisZ.Y * endOffset.Z, Z = transform.BasisZ.Z * endOffset.Z };
                            locationCurve = new BH.oM.Geometry.Line { Start = BH.Engine.Geometry.Modify.Translate(l.Start, yOffsetStart - zOffsetStart), End = BH.Engine.Geometry.Modify.Translate(l.End, yOffsetEnd - zOffsetEnd) };
                        }
                        else
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Offset/justification of nonlinear framing is currently not supported. Revit justification and offset has been ignored. Element Id: {0}", familyInstance.Id.IntegerValue));
                    }
                }
            }
            
            string materialGrade = familyInstance.MaterialGrade();
            IMaterialFragment materialFragment = familyInstance.StructuralMaterialType.LibraryMaterial(materialGrade);

            if (materialFragment == null)
            {
                //Compute.MaterialNotInLibraryNote(familyInstance);

                // Check if an instance or type Structural Material parameter exists.
                ElementId structuralMaterialId = familyInstance.StructuralMaterialId;
                if (structuralMaterialId.IntegerValue < 0)
                    structuralMaterialId = familyInstance.Symbol.LookupParameterElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

                materialFragment = (familyInstance.Document.GetElement(structuralMaterialId) as Autodesk.Revit.DB.Material).ToBHoMMaterialFragment(null, settings, refObjects);
            }

            if (materialFragment == null)
            {
                Compute.InvalidDataMaterialWarning(familyInstance);
                materialFragment = familyInstance.StructuralMaterialType.BHoMEmptyMaterialFragment(settings);
            }

            //TODO: this probably should be deleted - the Physical material in RefObjects has no properties by definition (it gets updated after being set to RefObjects)
            //if (materialFragment == null)
            //{
            //    BH.oM.Physical.Materials.Material material = pullSettings.FindRefObject<oM.Physical.Materials.Material>(familyInstance.StructuralMaterialId.IntegerValue);
            //    if (material != null)
            //        materialFragment = material.GetMaterialProperties(oM.Adapters.Revit.Enums.Discipline.Structural).FirstOrDefault() as IMaterialFragment;
            //    else
            //    {
            //        if (materialFragment == null)
            //        {
            //            //Compute.MaterialNotInLibraryNote(familyInstance);
            //            materialFragment = (familyInstance.Document.GetElement(familyInstance.StructuralMaterialId) as Autodesk.Revit.DB.Material).ToBHoMMaterialFragment(pullSettings);
            //        }

            //        if (materialFragment == null)
            //        {
            //            Compute.InvalidDataMaterialWarning(familyInstance);
            //            materialFragment = BHoMEmptyMaterialFragment(familyInstance.StructuralMaterialType, pullSettings);
            //        }
            //    }
            //}

            string elementName = familyInstance.Name;
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
            
            bars = new List<Bar>();

            if (locationCurve != null)
            {
                double rotation = familyInstance.FramingElementRotation(settings);
                foreach (BH.oM.Geometry.Line line in BH.Engine.Geometry.Query.SubParts(BH.Engine.Geometry.Modify.ICollapseToPolyline(locationCurve, Math.PI / 12)))
                {
                    //Bar bar = BH.Engine.Structure.Create.Bar(line, property, rotation);
                    //bar.Offset = new oM.Structure.Offsets.Offset { Start = startOffset, End = endOffset };
                    //bars.Add(bar);
                    bars.Add(BH.Engine.Structure.Create.Bar(line, property, rotation));
                }
            }
            else
                bars.Add(BH.Engine.Structure.Create.Bar(null, null, property, 0));

            for (int i = 0; i < bars.Count; i++)
            {
                bars[i].Name = elementName;

                //Set identifiers & custom data
                bars[i] = bars[i].SetIdentifiers(familyInstance) as Bar;
                bars[i] = bars[i].SetCustomData(familyInstance) as Bar;

                refObjects.AddOrReplace(familyInstance.Id, bars[i]);
            }
            
            return bars;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static void AnalyticalBarLocation(this AnalyticalModelStick analyticalModel, out oM.Geometry.ICurve locationCurve, RevitSettings settings = null)
        {
            locationCurve = null;

            Curve curve = analyticalModel.GetCurve();
            if (curve == null)
                return;

            locationCurve = curve.IToBHoM();
        }

        /***************************************************/
    }
}
