/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Environment.Fragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<oM.Architecture.Elements.Ceiling> ToBHoMCeilings(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Architecture.Elements.Ceiling> ceilingList = pullSettings.FindRefObjects<oM.Architecture.Elements.Ceiling>(ceiling.Id.IntegerValue);
            if (ceilingList != null && ceilingList.Count > 0)
                return ceilingList;

            oM.Physical.Constructions.Construction construction = ToBHoMConstruction(ceiling.Document.GetElement(ceiling.GetTypeId()) as HostObjAttributes, pullSettings);

            Dictionary<PlanarSurface, List<BH.oM.Physical.Elements.IOpening>> disctionary = Query.PlanarSurfaceDictionary(ceiling, false, pullSettings);
            if (disctionary == null)
                return null;

            ceilingList = new List<oM.Architecture.Elements.Ceiling>();
            foreach (PlanarSurface planarSurface in disctionary.Keys)
            {
                oM.Architecture.Elements.Ceiling newCeiling = new oM.Architecture.Elements.Ceiling()
                {
                    Name = Query.FamilyTypeFullName(ceiling),
                    Construction = construction,
                    Surface = planarSurface
                };

                ElementType elementType = ceiling.Document.GetElement(ceiling.GetTypeId()) as ElementType;
                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = ceiling.Id.IntegerValue.ToString();
                originContext.TypeName = Query.FamilyTypeFullName(ceiling);
                originContext = originContext.UpdateValues(pullSettings, ceiling) as OriginContextFragment;
                originContext = originContext.UpdateValues(pullSettings, elementType) as OriginContextFragment;
                newCeiling.Fragments.Add(originContext);

                newCeiling = Modify.SetIdentifiers(newCeiling, ceiling) as oM.Architecture.Elements.Ceiling;
                if (pullSettings.CopyCustomData)
                    newCeiling = Modify.SetCustomData(newCeiling, ceiling) as oM.Architecture.Elements.Ceiling;

                newCeiling = newCeiling.UpdateValues(pullSettings, ceiling) as oM.Architecture.Elements.Ceiling;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(newCeiling);

                ceilingList.Add(newCeiling);

            }

            return ceilingList;
        }

        /***************************************************/
    }
}