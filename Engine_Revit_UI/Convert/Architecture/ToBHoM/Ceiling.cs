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

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Environment.Fragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Architecture.Elements.Ceiling> ToBHoMCeilings(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Architecture.Elements.Ceiling> aCeilingList = pullSettings.FindRefObjects<oM.Architecture.Elements.Ceiling>(ceiling.Id.IntegerValue);
            if (aCeilingList != null && aCeilingList.Count > 0)
                return aCeilingList;

            oM.Physical.Constructions.Construction aConstruction = ToBHoMConstruction(ceiling.Document.GetElement(ceiling.GetTypeId()) as HostObjAttributes, pullSettings);

            IEnumerable<PlanarSurface> aPlanarSurfaces = Query.PlanarSurfaces(ceiling, pullSettings);
            if (aPlanarSurfaces == null)
                return null;

            aCeilingList = new List<oM.Architecture.Elements.Ceiling>();
            foreach (PlanarSurface aPlanarSurface in aPlanarSurfaces)
            {
                oM.Architecture.Elements.Ceiling aCeiling = new oM.Architecture.Elements.Ceiling()
                {
                    Name = Query.FamilyTypeFullName(ceiling),
                    Construction = aConstruction
                };

                ElementType aElementType = ceiling.Document.GetElement(ceiling.GetTypeId()) as ElementType;
                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = ceiling.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(ceiling);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, ceiling) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
                aCeiling.Fragments.Add(aOriginContextFragment);

                aCeiling = Modify.SetIdentifiers(aCeiling, ceiling) as oM.Architecture.Elements.Ceiling;
                if (pullSettings.CopyCustomData)
                    aCeiling = Modify.SetCustomData(aCeiling, ceiling, pullSettings.ConvertUnits) as oM.Architecture.Elements.Ceiling;

                aCeiling = aCeiling.UpdateValues(pullSettings, ceiling) as oM.Architecture.Elements.Ceiling;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aCeiling);

                aCeilingList.Add(aCeiling);

            }

            return aCeilingList;
        }

        /***************************************************/
    }
}