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

using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates Pull Settings class which contols pull behaviour of Adapter")]
        [Input("discipline", "Default disciplne for pull method")]
        [Input("refObjects", "Additional reference objects created during Pull process")]
        [Input("copyCustomData", "Saves Parameters of Revit Element into CustomData of BHoM Object")]
        [Input("convertUnits", "Converts units of parameters to SI")]
        [Output("PullSettings")]
        public static PullSettings PullSettings(Discipline discipline = Discipline.Physical, Dictionary<int, List<IBHoMObject>> refObjects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            PullSettings aPullSettings = new PullSettings()
            {
                Discipline = discipline,
                CopyCustomData = copyCustomData,
                ConvertUnits = convertUnits,
                RefObjects = refObjects
            };

            return aPullSettings;
        }

        /***************************************************/

        [Description("Creates Pull Settings class which contols pull behaviour of Adapter")]
        [Input("discipline", "Default disciplne for pull method")]
        [Input("mapSettings", "Mapping settings to be applied")]
        [Output("PullSettings")]
        public static PullSettings PullSettings(Discipline discipline, MapSettings mapSettings)
        {
            PullSettings aPullSettings = new PullSettings()
            {
                Discipline = discipline,
                MapSettings = mapSettings,
                RefObjects = new Dictionary<int, List<IBHoMObject>>(),
            };

            return aPullSettings;
        }

        /***************************************************/
    }
}
