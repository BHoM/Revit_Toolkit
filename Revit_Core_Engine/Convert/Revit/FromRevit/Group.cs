/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Group to BH.oM.Adapters.Revit.Elements.Group.")]
        [Input("group", "Revit Group to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("group", "BH.oM.Adapters.Revit.Elements.Group resulting from converting the input Revit Group.")]
        public static BH.oM.Adapters.Revit.Elements.Group GroupFromRevit(this Autodesk.Revit.DB.Group revitGroup, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            BH.oM.Adapters.Revit.Elements.Group group = refObjects.GetValue<BH.oM.Adapters.Revit.Elements.Group>(revitGroup.Id);
            if (group != null)
                return group;

            group = new BH.oM.Adapters.Revit.Elements.Group { Name = revitGroup.Name };
            foreach (ElementId memberId in revitGroup.GetMemberIds())
            {
                List<IBHoMObject> members;
                if (refObjects == null || !refObjects.TryGetValue(memberId.ToString(), out members))
                {
                    BH.Engine.Base.Compute.RecordError("Group could not be converted from Revit because not all of its members were converted prior to it." +
                                                       "\nPlease make sure all member elements of a group get converted to BHoM and cached in refObjects before conversion of the instance itself.");
                    return null;
                }

                group.MemberElements.AddRange(members);
            }

            //Set identifiers, parameters & custom data
            group.SetIdentifiers(revitGroup);
            group.CopyParameters(revitGroup, settings.MappingSettings);
            group.SetProperties(revitGroup, settings.MappingSettings);

            refObjects.AddOrReplace(revitGroup.Id, group);
            return group;
        }

        /***************************************************/
    }
}
