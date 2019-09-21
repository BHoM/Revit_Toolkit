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

using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Adapters.Revit.Settings;
using BH.Engine.Structure;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static ISurfaceProperty ToBHoMSurfaceProperty(this HostObjAttributes hostObjAttributes, PullSettings pullSettings = null, string materialGrade = null)
        {
            Document document = hostObjAttributes.Document;

            pullSettings = pullSettings.DefaultIfNull();

            ConstantThickness aConstantThickness = pullSettings.FindRefObject<ConstantThickness>(hostObjAttributes.Id.IntegerValue);
            if (aConstantThickness != null)
                return aConstantThickness;

            double aThickness = 0;
            IMaterialFragment materialFragment = null;

            bool composite = false;
            foreach (CompoundStructureLayer csl in hostObjAttributes.GetCompoundStructure().GetLayers())
            {
                if (csl.Function == MaterialFunctionAssignment.Structure)
                {
                    Material revitMaterial = document.GetElement(csl.MaterialId) as Material;
                    if (revitMaterial == null)
                    {
                        //TODO: warning about a structural layer skipped and skip
                    }

                    if (aThickness != 0)
                    {
                        composite = true;
                        aThickness = 0;
                        break;
                    }

                    aThickness = csl.Width;
                    if (pullSettings.ConvertUnits)
                        aThickness = aThickness.ToSI(UnitType.UT_Section_Dimension);

                    Autodesk.Revit.DB.Structure.StructuralMaterialType structuralMaterialType = revitMaterial.MaterialClass.StructuralMaterialType();
                    materialFragment = Query.LibraryMaterial(structuralMaterialType, materialGrade);

                    if (materialFragment == null)
                    {
                        //Compute.MaterialNotInLibraryNote(familyInstance);
                        materialFragment = revitMaterial.ToBHoMMaterialFragment(pullSettings);
                    }

                    if (materialFragment == null)
                    {
                        Compute.InvalidDataMaterialWarning(hostObjAttributes);
                        materialFragment = BHoMEmptyMaterialFragment(structuralMaterialType, pullSettings);
                    }
                }
                else if (csl.Function == MaterialFunctionAssignment.StructuralDeck)
                {
                    //TODO: warning that a composite deck here + action appropriately
                }
                else
                {
                    //TODO: warning that nonstructural layers exist and are skipped
                }
            }

            //TODO: Clean it up!
            if (composite)
                hostObjAttributes.CompositePanelWarning();
            else if (aThickness == 0)
                BH.Engine.Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", hostObjAttributes.Id.IntegerValue));

            oM.Structure.SurfaceProperties.PanelType panelType = oM.Structure.SurfaceProperties.PanelType.Undefined;
            if (hostObjAttributes is WallType)
                panelType = oM.Structure.SurfaceProperties.PanelType.Wall;
            else if (hostObjAttributes is FloorType)
                panelType = oM.Structure.SurfaceProperties.PanelType.Slab;

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = panelType, Thickness = aThickness, Material = materialFragment, Name = hostObjAttributes.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, hostObjAttributes) as ConstantThickness;
            if (pullSettings.CopyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, hostObjAttributes, pullSettings.ConvertUnits) as ConstantThickness;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aProperty2D);

            return aProperty2D;
        }

        /***************************************************/

        //internal static ISurfaceProperty ToBHoMSurfaceProperty(this FloorType floorType, PullSettings pullSettings = null)
        //{
        //    Document document = floorType.Document;

        //    pullSettings = pullSettings.DefaultIfNull();

        //    ConstantThickness aConstantThickness = pullSettings.FindRefObject<ConstantThickness>(floorType.Id.IntegerValue);
        //    if (aConstantThickness != null)
        //        return aConstantThickness;

        //    double aThickness = 0;
        //    oM.Physical.Materials.Material aMaterial = null;

        //    bool composite = false;
        //    foreach (CompoundStructureLayer csl in floorType.GetCompoundStructure().GetLayers())
        //    {
        //        if (csl.Function == MaterialFunctionAssignment.Structure)
        //        {
        //            if (aThickness != 0)
        //            {
        //                composite = true;
        //                aThickness = 0;
        //                break;
        //            }
        //            aThickness = csl.Width;
        //            if (pullSettings.ConvertUnits) aThickness = aThickness.ToSI(UnitType.UT_Section_Dimension);

        //            Material aMaterial_Revit = ElementId.InvalidElementId == csl.MaterialId ? floorType.Category.Material : document.GetElement(csl.MaterialId) as Material;

        //            //TODO: uncomment it!
        //            //aMaterial = aMaterial_Revit.ToBHoMMaterial(pullSettings);
        //        }
        //    }

        //    if (composite) floorType.CompositePanelWarning();
        //    else if (aThickness == 0) BH.Engine.Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", floorType.Id.IntegerValue));

        //    //Get out the structural material fragment. If no present settign to concrete for now
        //    IMaterialFragment strucMaterialFragment;
        //    if (aMaterial.IsValidStructural())
        //        strucMaterialFragment = aMaterial.StructuralMaterialFragment();
        //    else
        //    {
        //        strucMaterialFragment = new Concrete() { Name = aMaterial.Name };
        //        aMaterial.MaterialNotStructuralWarning();
        //    }


        //    ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structure.SurfaceProperties.PanelType.Slab, Thickness = aThickness, Material = strucMaterialFragment, Name = floorType.Name };

        //    aProperty2D = Modify.SetIdentifiers(aProperty2D, floorType) as ConstantThickness;
        //    if (pullSettings.CopyCustomData)
        //        aProperty2D = Modify.SetCustomData(aProperty2D, floorType, pullSettings.ConvertUnits) as ConstantThickness;

        //    pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aProperty2D);

        //    return aProperty2D;
        //}

        ///***************************************************/
    }
}