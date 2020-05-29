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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<oM.Architecture.Elements.Ceiling> CeilingsFromRevit(this Ceiling ceiling, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Architecture.Elements.Ceiling> ceilingList = refObjects.GetValues<oM.Architecture.Elements.Ceiling>(ceiling.Id);
            if (ceilingList != null && ceilingList.Count != 0)
                return ceilingList;

            oM.Physical.Constructions.Construction construction = (ceiling.Document.GetElement(ceiling.GetTypeId()) as HostObjAttributes).ConstructionFromRevit(settings, refObjects);

            Dictionary<PlanarSurface, List<BH.oM.Physical.Elements.IOpening>> disctionary = ceiling.PlanarSurfaceDictionary(false, settings);
            if (disctionary == null)
                return null;

            ceilingList = new List<oM.Architecture.Elements.Ceiling>();
            foreach (PlanarSurface planarSurface in disctionary.Keys)
            {
                oM.Architecture.Elements.Ceiling newCeiling = new oM.Architecture.Elements.Ceiling()
                {
                    Name = ceiling.FamilyTypeFullName(),
                    Construction = construction,
                    Surface = planarSurface
                };

                ElementType elementType = ceiling.Document.GetElement(ceiling.GetTypeId()) as ElementType;

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = ceiling.Id.IntegerValue.ToString();
                originContext.TypeName = ceiling.FamilyTypeFullName();
                originContext.SetProperties(ceiling, settings.ParameterSettings);
                originContext.SetProperties(elementType, settings.ParameterSettings);
                newCeiling.Fragments.Add(originContext);

                //Set identifiers, parameters & custom data
                newCeiling.SetIdentifiers(ceiling);
                newCeiling.SetCustomData(ceiling, settings.ParameterSettings);
                newCeiling.SetProperties(ceiling, settings.ParameterSettings);

                newCeiling.CustomData.Add(BH.Engine.Adapters.Revit.Convert.CeilingPattern, ceiling.CeilingPattern(settings, planarSurface));
                if (disctionary.Values.Where(x => x != null).Sum(x => x.Count) != 0)
                    BH.Engine.Reflection.Compute.RecordWarning("Currently ceiling openings are not taken into account when generating ceilings.");

                ceilingList.Add(newCeiling);
            }

            refObjects.AddOrReplace(ceiling.Id, ceilingList);
            return ceilingList;
        }

        /***************************************************/
    }
}
