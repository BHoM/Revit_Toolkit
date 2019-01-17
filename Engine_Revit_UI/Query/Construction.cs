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

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public oM.Environment.Elements.Construction Construction(this HostObjAttributes hostObjAttributes, PullSettings pullSettings = null)
        {
            if (hostObjAttributes == null)
                return null;

            CompoundStructure aCompoundStructure = hostObjAttributes.GetCompoundStructure();
            if (aCompoundStructure == null)
                return null;

            string aName = hostObjAttributes.FamilyTypeFullName();
            aName = aName.Replace(" ", string.Empty);
            aName = aName.Replace(":", "_");

            return Construction(aCompoundStructure, aName, hostObjAttributes.Document, (BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue, pullSettings);
        }

        /***************************************************/

        static public oM.Environment.Elements.Construction Construction(this CompoundStructure compoundStructure, string Name, Document document, BuiltInCategory BuiltInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID, PullSettings pullSettings = null)
        {
            if (compoundStructure == null)
                return null;

            IEnumerable<CompoundStructureLayer> aCompoundStructureLayers = compoundStructure.GetLayers();
            if (aCompoundStructureLayers == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            oM.Environment.Elements.Construction aConstruction = new oM.Environment.Elements.Construction();
            aConstruction.Name = Name;
            foreach (CompoundStructureLayer aCompoundStructureLayer in aCompoundStructureLayers)
                aConstruction.Materials.Add(Query.Material(aCompoundStructureLayer, document, BuiltInCategory, pullSettings));

            return aConstruction;
        }

        /***************************************************/
    }
}