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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using Revision = BH.oM.Adapters.Revit.Elements.Revision;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Revision to BH.oM.Adapters.Revit.Elements.Revision.")]
        [Input("revision", "Revit Revision to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("revision", "BH.oM.Adapters.Revit.Elements.Revision resulting from converting the input Revit Revision.")]
        public static Revision RevisionFromRevit(this Autodesk.Revit.DB.Revision revision, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Revision rev = refObjects.GetValue<Revision>(revision.Id);
            if (rev != null)
                return rev;

            rev = BH.Engine.Adapters.Revit.Create.Revision(revision.RevisionDate, revision.Description);

            ElementType elementType = revision.Document.GetElement(revision.GetTypeId()) as ElementType;
            if (elementType != null)
                rev.InstanceProperties = elementType.InstancePropertiesFromRevit(settings, refObjects);
            
            rev.Name = revision.Description;
            
            //Set identifiers, parameters & custom data
            rev.SetIdentifiers(element: revision);
            rev.CopyParameters(revision, settings.MappingSettings);
            rev.SetProperties(revision, settings.MappingSettings);

            refObjects.AddOrReplace(revision.Id, rev);
            return rev;
        }

        /***************************************************/
    }
}
