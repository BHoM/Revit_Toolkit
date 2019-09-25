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
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;
using System.Linq;
using System;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Constructions;
using BH.oM.Physical;
using BH.oM.Physical.Materials;
using BH.oM.Environment.MaterialFragments;
using BH.oM.Structure.MaterialFragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        internal static BH.oM.Physical.Constructions.Construction UpdateMaterialProperties(this BH.oM.Physical.Constructions.Construction construction, HostObjAttributes hostObjectAttributes, string materialGrade, PullSettings pullSettings)
        {
            BH.oM.Physical.Constructions.Construction newConstruction = construction.GetShallowClone() as BH.oM.Physical.Constructions.Construction;
            newConstruction.Layers = new List<Layer>(construction.Layers);

            CompoundStructure compoundStructure = hostObjectAttributes.GetCompoundStructure();
            if (compoundStructure == null)
                {
                    //throw error
                    return newConstruction;
                }

            List<CompoundStructureLayer> revitLayers = compoundStructure.GetLayers().ToList();
            if (newConstruction.Layers.Count!= revitLayers.Count)
            {
                //throw error
                return newConstruction;
            }

            Document doc = hostObjectAttributes.Document;
            for (int i = 0; i < construction.Layers.Count; i++)
            {
                Layer newLayer = construction.Layers[i].GetShallowClone() as Layer;

                CompoundStructureLayer revitLayer = revitLayers[i];
                Autodesk.Revit.DB.Material revitMaterial = doc.GetElement(revitLayers[i].MaterialId) as Autodesk.Revit.DB.Material;
                //BH.oM.Physical.Materials.Material material = construction.Layers[i].Material;

                newLayer.Material = newLayer.Material.UpdateMaterialProperties(revitMaterial, pullSettings, materialGrade);
                newConstruction.Layers[i] = newLayer;
            }

            return newConstruction;
        }

        internal static BH.oM.Physical.Materials.Material UpdateMaterialProperties(this BH.oM.Physical.Materials.Material material, Autodesk.Revit.DB.Material sourceMaterial, PullSettings pullSettings, string materialGrade = null, StructuralMaterialType structuralMaterialType = StructuralMaterialType.Undefined)
        {
            BH.oM.Physical.Materials.Material newMaterial = material.GetShallowClone() as BH.oM.Physical.Materials.Material;
            newMaterial.Properties = sourceMaterial.ToBHoMMaterialProperties(pullSettings, materialGrade);

            if (sourceMaterial == null)
            {
                if (structuralMaterialType == StructuralMaterialType.Undefined)
                    newMaterial.Name = "Unknown Material";
                else
                {
                    newMaterial.Name = String.Format("Unknown {0} Material", structuralMaterialType);
                    newMaterial.TryUpdateEmptyMaterialFromLibrary(structuralMaterialType, pullSettings, materialGrade);
                }
            }
            else
                newMaterial.Name = sourceMaterial.Name;

            return newMaterial;
        }

        internal static bool TryUpdateEmptyMaterialFromLibrary(this BH.oM.Physical.Materials.Material material, StructuralMaterialType structuralMaterialType, PullSettings pullSettings, string materialGrade = null)
        {
            IMaterialFragment structuralProperty = Query.LibraryMaterial(structuralMaterialType, materialGrade);
            if (structuralProperty != null)
            {
                material.Properties.Add(structuralProperty);
                material.Name = structuralProperty.Name;
                return true;
            }
            else
            {
                switch (structuralMaterialType)
                {
                    case StructuralMaterialType.Aluminum:
                        structuralProperty = new BH.oM.Structure.MaterialFragments.Aluminium();
                        break;
                    case StructuralMaterialType.Concrete:
                    case StructuralMaterialType.PrecastConcrete:
                        structuralProperty = new BH.oM.Structure.MaterialFragments.Concrete();
                        break;
                    case StructuralMaterialType.Wood:
                        structuralProperty = new BH.oM.Structure.MaterialFragments.Timber();
                        break;
                    default:
                        structuralProperty = new BH.oM.Structure.MaterialFragments.Steel();
                        break;
                }

                structuralProperty.Name = String.Format("Unknown {0} Material", structuralMaterialType);
                material.Properties.Add(structuralProperty);
                return false;
            }
        }

        internal static List<IMaterialProperties> ToBHoMMaterialProperties(this Autodesk.Revit.DB.Material material, PullSettings pullSettings, string materialGrade = null)
        {
            List<IMaterialProperties> result = new List<IMaterialProperties>();
            List<Type> types = pullSettings.Discipline.MaterialTypes();
            foreach (Type type in types)
            {
                IMaterialProperties properties = material.ToBHoMMaterialProperties(type, pullSettings, materialGrade);
                if (properties != null)
                    result.Add(properties);
            }

            return result;
        }

        /***************************************************/

        private static IMaterialProperties ToBHoMMaterialProperties(this Autodesk.Revit.DB.Material material, Type type, PullSettings pullSettings, string materialGrade = null)
        {
            if (type == typeof(SolidMaterial))
                return material.ToBHoMSolidMaterial(pullSettings);
            else if (type == typeof(IMaterialFragment))
                return material.ToBHoMMaterialFragment(pullSettings, materialGrade);
            else
                return null;
        }

        /***************************************************/

        private static List<Type> MaterialTypes(this oM.Adapters.Revit.Enums.Discipline discipline)
        {
            switch (discipline)
            {
                case oM.Adapters.Revit.Enums.Discipline.Physical:
                    return new List<Type> { typeof(SolidMaterial), typeof(IMaterialFragment) };
                case oM.Adapters.Revit.Enums.Discipline.Structural:
                    return new List<Type> { typeof(IMaterialFragment) };
                case oM.Adapters.Revit.Enums.Discipline.Environmental:
                    return new List<Type> { typeof(SolidMaterial) };
                default:
                    return new List<Type>();
            }
        }

        /***************************************************/

        private static bool ContainsDisciplineProperties(this oM.Physical.Materials.Material material, oM.Adapters.Revit.Enums.Discipline discipline)
        {
            List<Type> types = discipline.MaterialTypes();
            return material.Properties.All(x => types.Contains(x.GetType()));
        }

        /***************************************************/

        internal static List<IMaterialProperties> GetMaterialProperties(this oM.Physical.Materials.Material material, oM.Adapters.Revit.Enums.Discipline discipline)
        {
            List<IMaterialProperties> result = new List<IMaterialProperties>();
            foreach (Type type in discipline.MaterialTypes())
            {
                IMaterialProperties properties = material.GetMaterialProperties(type);
                if (properties != null)
                    result.Add(properties);
            }

            return result;
        }

        /***************************************************/

        private static IMaterialProperties GetMaterialProperties(this oM.Physical.Materials.Material material, Type type)
        {
            foreach (IMaterialProperties properties in material.Properties)
            {
                if (properties.GetType() == type)
                {
                    //TODO: is that copy OK?
                    IMaterialProperties result = properties.GetShallowClone() as IMaterialProperties;
                    result.CustomData = new Dictionary<string, object>(material.CustomData);
                    return result;
                }
            }

            return null;
        }

        /***************************************************/
    }
}
