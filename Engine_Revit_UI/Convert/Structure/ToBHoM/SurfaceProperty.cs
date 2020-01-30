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
        /****               Public Methods              ****/
        /***************************************************/

        public static ISurfaceProperty ToBHoMSurfaceProperty(this HostObjAttributes hostObjAttributes, PullSettings pullSettings = null, string materialGrade = null)
        {
            Document document = hostObjAttributes.Document;

            pullSettings = pullSettings.DefaultIfNull();

            ConstantThickness constantThickness = pullSettings.FindRefObject<ConstantThickness>(hostObjAttributes.Id.IntegerValue);
            if (constantThickness != null)
                return constantThickness;

            double thickness = 0;
            IMaterialFragment materialFragment = null;

            bool composite = false;
            foreach (CompoundStructureLayer csl in hostObjAttributes.GetCompoundStructure().GetLayers())
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
                    {
                        BH.Engine.Reflection.Compute.RecordWarning("There is a structural layer in wall/floor type without material assigned. A default empty material is returned. ElementId: " + hostObjAttributes.Id.IntegerValue.ToString());
                        materialFragment = BHoMEmptyMaterialFragment(Autodesk.Revit.DB.Structure.StructuralMaterialType.Undefined, pullSettings);
                    }
                    else
                    {
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
            else if (thickness == 0)
                BH.Engine.Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", hostObjAttributes.Id.IntegerValue));

            oM.Structure.SurfaceProperties.PanelType panelType = oM.Structure.SurfaceProperties.PanelType.Undefined;
            if (hostObjAttributes is WallType)
                panelType = oM.Structure.SurfaceProperties.PanelType.Wall;
            else if (hostObjAttributes is FloorType)
                panelType = oM.Structure.SurfaceProperties.PanelType.Slab;

            ConstantThickness property2D = new ConstantThickness { PanelType = panelType, Thickness = thickness, Material = materialFragment, Name = hostObjAttributes.Name };

            property2D = Modify.SetIdentifiers(property2D, hostObjAttributes) as ConstantThickness;
            if (pullSettings.CopyCustomData)
                property2D = Modify.SetCustomData(property2D, hostObjAttributes) as ConstantThickness;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(property2D);

            return property2D;
        }

        /***************************************************/
    }
}
