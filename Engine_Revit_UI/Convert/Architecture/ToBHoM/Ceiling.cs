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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
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

        public static List<oM.Architecture.Elements.Ceiling> ToBHoMCeilings(this Ceiling ceiling, RevitSettings settings = null, Dictionary<int, List<IBHoMObject>> refObjects = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Architecture.Elements.Ceiling> ceilingList = refObjects.GetValues<oM.Architecture.Elements.Ceiling>(ceiling.Id);
            if (ceilingList != null && ceilingList.Count != 0)
                return ceilingList;

            oM.Physical.Constructions.Construction construction = ToBHoMConstruction(ceiling.Document.GetElement(ceiling.GetTypeId()) as HostObjAttributes, settings, refObjects);

            Dictionary<PlanarSurface, List<BH.oM.Physical.Elements.IOpening>> disctionary = Query.PlanarSurfaceDictionary(ceiling, false, settings, refObjects);
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
                originContext = originContext.UpdateValues(settings, ceiling) as OriginContextFragment;
                originContext = originContext.UpdateValues(settings, elementType) as OriginContextFragment;
                newCeiling.Fragments.Add(originContext);

                newCeiling = Modify.SetIdentifiers(newCeiling, ceiling) as oM.Architecture.Elements.Ceiling;
                newCeiling = Modify.SetCustomData(newCeiling, ceiling) as oM.Architecture.Elements.Ceiling;

                newCeiling = newCeiling.UpdateValues(settings, ceiling) as oM.Architecture.Elements.Ceiling;
                

                ceilingList.Add(newCeiling);

            }

            refObjects.Add(ceiling.Id.IntegerValue, ceilingList.Cast<IBHoMObject>().ToList());
            return ceilingList;
        }

        /***************************************************/
    }
}
