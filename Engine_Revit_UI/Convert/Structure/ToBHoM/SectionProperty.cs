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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.SectionProperties;
using BH.oM.Geometry.ShapeProfiles;
using BH.Engine.Structure;
using BH.oM.Structure.MaterialFragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static ISectionProperty ToBHoMSectionProperty(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            ISectionProperty aSectionProperty = pullSettings.FindRefObject<ISectionProperty>(familyInstance.Id.IntegerValue);
            if (aSectionProperty != null)
                return aSectionProperty;

            oM.Physical.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Physical.Materials.Material>(familyInstance.StructuralMaterialId.IntegerValue);

            if (aMaterial == null)
            {
                ElementId materialId = familyInstance.StructuralMaterialId;

                //TODO: can materialId == -1 && StructuralMaterialType and MaterialGrade be assigned?
                if (materialId.IntegerValue == -1)
                {
                    string materialGrade = familyInstance.MaterialGrade();
                    aMaterial = Query.LibraryMaterial(familyInstance.StructuralMaterialType, materialGrade);
                }

                if (aMaterial == null && materialId.IntegerValue != -1)
                {
                    Autodesk.Revit.DB.Material aMaterial_Revit = familyInstance.Document.GetElement(materialId) as Autodesk.Revit.DB.Material;
                    if (aMaterial_Revit != null)
                        aMaterial = aMaterial_Revit.ToBHoMMaterial(pullSettings);
                }

                if (aMaterial == null)
                {
                    Compute.InvalidDataMaterialWarning(familyInstance);
                    Compute.MaterialTypeNotFoundWarning(familyInstance);
                    aMaterial = new oM.Physical.Materials.Material();
                }
            }

            string symbolName = familyInstance.Symbol.Name;
            aSectionProperty = BH.Engine.Library.Query.Match("SectionProperties", symbolName) as ISectionProperty;

            //Get out the structural material fragment. If no present settign to steel for now
            IMaterialFragment strucMaterialFragment;
            if (aMaterial.IsValidStructural())
                strucMaterialFragment = aMaterial.StructuralMaterialFragment();
            else
            {
                strucMaterialFragment = new Steel() { Name = aMaterial.Name };
                aMaterial.MaterialNotStructuralWarning();
            }

            if (aSectionProperty != null)
            {
                aSectionProperty = aSectionProperty.GetShallowClone() as ISectionProperty;

                aSectionProperty.Material = strucMaterialFragment;
                aSectionProperty.Name = symbolName;
            }
            else
            {
                IProfile aSectionDimensions = familyInstance.Symbol.ToBHoMProfile(pullSettings);

                if (aSectionDimensions.Edges.Count == 0)
                {
                    List<oM.Geometry.ICurve> profileCurves = new List<oM.Geometry.ICurve>();
                    if (familyInstance.HasSweptProfile())
                    {
                        profileCurves = familyInstance.GetSweptProfile().GetSweptProfile().Curves.ToBHoM(pullSettings);
                    }
                    else
                    {
                        foreach (GeometryObject obj in familyInstance.Symbol.get_Geometry(new Options()))
                        {
                            if (obj is Solid)
                            {
                                XYZ direction = familyInstance.StructuralType == StructuralType.Column ? new XYZ(0, 0, 1) : new XYZ(1, 0, 0);
                                foreach (Face face in (obj as Solid).Faces)
                                {
                                    if (face is PlanarFace && (face as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(direction, 0.001) || (face as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(-direction, 0.001))
                                    {
                                        foreach (EdgeArray curveArray in (face as PlanarFace).EdgeLoops)
                                        {
                                            foreach (Edge c in curveArray)
                                            {
                                                profileCurves.Add(c.AsCurve().ToBHoM(pullSettings));
                                            }
                                        }
                                        break;
                                    }
                                }


                                //Checking if the curves are in the horisontal plane, if not roating them.
                                BH.oM.Geometry.Point dirPt = direction.ToBHoM(pullSettings);
                                BH.oM.Geometry.Vector tan = new oM.Geometry.Vector { X = dirPt.X, Y = dirPt.Y, Z = dirPt.Z };

                                double angle = BH.Engine.Geometry.Query.Angle(tan, BH.oM.Geometry.Vector.ZAxis);

                                if (angle > BH.oM.Geometry.Tolerance.Angle)
                                {
                                    BH.oM.Geometry.Vector rotAxis = BH.Engine.Geometry.Query.CrossProduct(tan, BH.oM.Geometry.Vector.ZAxis);

                                    profileCurves = profileCurves.Select(x => BH.Engine.Geometry.Modify.IRotate(x, oM.Geometry.Point.Origin, rotAxis, angle)).ToList();
                                }
                                //

                                break;
                            }
                        }
                    }

                    aSectionDimensions = new FreeFormProfile(profileCurves);

                    aSectionDimensions = Modify.SetIdentifiers(aSectionDimensions, familyInstance.Symbol) as IProfile;
                    if (pullSettings.CopyCustomData)
                        aSectionDimensions = Modify.SetCustomData(aSectionDimensions, familyInstance.Symbol, pullSettings.ConvertUnits) as IProfile;

                    pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSectionDimensions);
                }

                string name = familyInstance.Name;
                bool emptyProfile = aSectionDimensions.Edges.Count == 0;
                if (emptyProfile) familyInstance.Symbol.ConvertProfileFailedWarning();

                //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                if (strucMaterialFragment == null)
                {
                    familyInstance.UnknownMaterialWarning();
                    if (emptyProfile)
                    {
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        aSectionProperty.Name = name;
                    }
                    else aSectionProperty = BH.Engine.Structure.Create.SteelSectionFromProfile(aSectionDimensions, strucMaterialFragment as Steel, name);
                }
                else if (strucMaterialFragment is Concrete)
                {
                    if (emptyProfile)
                    {
                        aSectionProperty = new ConcreteSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        aSectionProperty.Name = name;
                    }
                    else aSectionProperty = BH.Engine.Structure.Create.ConcreteSectionFromProfile(aSectionDimensions, strucMaterialFragment as Concrete, name);
                }
                else if (strucMaterialFragment is Steel)
                {
                    if (emptyProfile)
                    {
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        aSectionProperty.Name = name;
                    }
                    else aSectionProperty = BH.Engine.Structure.Create.SteelSectionFromProfile(aSectionDimensions, strucMaterialFragment as Steel, name);
                }
                else
                {
                    familyInstance.UnknownMaterialWarning();
                    if (emptyProfile)
                    {
                        aSectionProperty = new SteelSection(aSectionDimensions, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        aSectionProperty.Name = name;
                    }
                    else
                    {
                        aSectionProperty = BH.Engine.Structure.Create.SteelSectionFromProfile(aSectionDimensions, null, name);
                        aSectionProperty.Material = strucMaterialFragment;
                    } 
                }
            }

            aSectionProperty = Modify.SetIdentifiers(aSectionProperty, familyInstance) as ISectionProperty;
            if (pullSettings.CopyCustomData)
                aSectionProperty = Modify.SetCustomData(aSectionProperty, familyInstance, pullSettings.ConvertUnits) as ISectionProperty;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSectionProperty);

            return aSectionProperty;
        }

        /***************************************************/
    }
}