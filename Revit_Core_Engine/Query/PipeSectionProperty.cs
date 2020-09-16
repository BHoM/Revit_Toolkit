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
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.MEP.SectionProperties;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Query a Revit pipe to get a BHoM pipe section property.")]
        [Input("pipe", "Revit pipe to be converted into a BHoM section property.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("sectionProperty", "BHoM pipe section property extracted from a Revit pipe.")]
        public static BH.oM.MEP.SectionProperties.PipeSectionProperty PipeSectionProperty(this Autodesk.Revit.DB.Plumbing.Pipe pipe, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            IProfile profile = pipe.Profile(settings, refObjects);

            double liningThickness = pipe.LookupParameterDouble(BuiltInParameter.RBS_REFERENCE_LINING_THICKNESS); // Extract the lining thk from Duct element
            if (liningThickness == 0)
                liningThickness = BH.oM.Geometry.Tolerance.Distance;


            double insulationThickness = pipe.LookupParameterDouble(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS); // Extract the lining thk from Duct element
            if (insulationThickness == 0)
                insulationThickness = BH.oM.Geometry.Tolerance.Distance;

            SectionProfile sectionProfile = BH.Engine.MEP.Create.SectionProfile((TubeProfile)profile, liningThickness, insulationThickness);

            PipeSectionProperty result = BH.Engine.MEP.Create.PipeSectionProperty(sectionProfile);

            return result;
        }

        /***************************************************/
    }
}